using MolServiceContracts.ViewModels.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LaboratoryHeadApp.Services
{
    public class InventoryReportPdfService : IInventoryReportPdfService
    {
        public byte[] GenerateFullInventoryPdf(FullInventoryReportViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var items = model.Items?
                .Where(x => x.Quantity != 0)
                .ToList() ?? new List<InventoryReportItemViewModel>();

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(14);
                    page.DefaultTextStyle(x => x.FontSize(7));

                    page.Content().Column(column =>
                    {
                        column.Item().Element(ComposeFullInventoryHeader);

                        column.Item()
                            .PaddingTop(6)
                            .Text("1. Сведения об объектах нефинансовых активов по данным бухгалтерского учета")
                            .FontSize(7)
                            .Bold();

                        if (items.Count == 0)
                        {
                            column.Item()
                                .Border(1)
                                .BorderColor(Colors.Black)
                                .Padding(8)
                                .Text("Данные для отображения отсутствуют.");
                            return;
                        }

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(38);      // 1 Код строки
                                columns.RelativeColumn(3.6f);   // 2 Наименование
                                columns.ConstantColumn(120);     // 3 Инвентарный номер
                                columns.RelativeColumn(2.4f);    // 4 Место проведения
                                columns.ConstantColumn(65);      // 5 Количество
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(FormHeaderCellStyle).Text("Код строки");
                                header.Cell().Element(FormHeaderCellStyle).Text("Наименование объекта нефинансового актива");
                                header.Cell().Element(FormHeaderCellStyle).Text("Номер (код) объекта учета (инвентарный или иной)");
                                header.Cell().Element(FormHeaderCellStyle).Text("Место / подразделение проведения инвентаризации");
                                header.Cell().Element(FormHeaderCellStyle).Text("Количество");

                                header.Cell().Element(ColumnNumberCellStyle).Text("1");
                                header.Cell().Element(ColumnNumberCellStyle).Text("2");
                                header.Cell().Element(ColumnNumberCellStyle).Text("3");
                                header.Cell().Element(ColumnNumberCellStyle).Text("4");
                                header.Cell().Element(ColumnNumberCellStyle).Text("5");
                            });

                            for (int i = 0; i < items.Count; i++)
                            {
                                var item = items[i];

                                table.Cell().Element(FormBodyCellStyle).AlignCenter().Text((i + 1).ToString());
                                table.Cell().Element(FormBodyCellStyle).Text(item.FullName ?? string.Empty);
                                table.Cell().Element(FormBodyCellStyle).Text(item.InventoryNumber ?? string.Empty);
                                table.Cell().Element(FormBodyCellStyle).Text(GetInventoryPlace(item));
                                table.Cell().Element(FormBodyCellStyle).AlignCenter().Text(FormatQuantity(item.Quantity));
                            }
                        });
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Страница ");
                            x.CurrentPageNumber();
                            x.Span(" из ");
                            x.TotalPages();
                        });
                });
            }).GeneratePdf();
        }

        public byte[] GenerateClassroomsInventoryPdf(ClassroomsInventoryReportViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(column =>
                    {
                        column.Item().Text("Отчёт по выбранным аудиториям")
                            .FontSize(18)
                            .Bold();

                        column.Item().PaddingTop(4)
                            .Text($"Дата формирования: {model.CreatedAt:dd.MM.yyyy HH:mm}");
                    });

                    page.Content().PaddingTop(12).Column(column =>
                    {
                        if (model.Classrooms == null || model.Classrooms.Count == 0)
                        {
                            column.Item().Text("Нет данных для отображения.");
                            return;
                        }

                        for (int c = 0; c < model.Classrooms.Count; c++)
                        {
                            var classroom = model.Classrooms[c];

                            column.Item().PaddingBottom(8).Text($"Аудитория {classroom.ClassroomNumber}")
                                .FontSize(14)
                                .Bold();

                            if (classroom.Items == null || classroom.Items.Count == 0)
                            {
                                column.Item()
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten1)
                                    .Padding(8)
                                    .Text("Для этой аудитории оборудование не найдено.");

                                if (c < model.Classrooms.Count - 1)
                                {
                                    column.Item().PaddingBottom(14);
                                }

                                continue;
                            }

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(28);
                                    columns.ConstantColumn(95);
                                    columns.RelativeColumn(3);
                                    columns.ConstantColumn(50);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("№");
                                    header.Cell().Element(HeaderCellStyle).Text("Инв. номер");
                                    header.Cell().Element(HeaderCellStyle).Text("Наименование");
                                    header.Cell().Element(HeaderCellStyle).Text("Кол-во");
                                    header.Cell().Element(HeaderCellStyle).Text("МОЛ");
                                    header.Cell().Element(HeaderCellStyle).Text("Описание");
                                });

                                for (int i = 0; i < classroom.Items.Count; i++)
                                {
                                    var item = classroom.Items[i];

                                    table.Cell().Element(BodyCellStyle).Text((i + 1).ToString());
                                    table.Cell().Element(BodyCellStyle).Text(item.InventoryNumber ?? string.Empty);
                                    table.Cell().Element(BodyCellStyle).Text(item.FullName ?? string.Empty);
                                    table.Cell().Element(BodyCellStyle).Text(item.Quantity.ToString());
                                    table.Cell().Element(BodyCellStyle).Text(item.MaterialResponsiblePersonName ?? string.Empty);
                                    table.Cell().Element(BodyCellStyle).Text(item.Description ?? string.Empty);
                                }
                            });

                            if (c < model.Classrooms.Count - 1)
                            {
                                column.Item().PaddingBottom(16);
                            }
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Страница ");
                            x.CurrentPageNumber();
                            x.Span(" из ");
                            x.TotalPages();
                        });
                });
            }).GeneratePdf();
        }

        private static void ComposeFullInventoryHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().PaddingRight(14).Column(left =>
                    {
                        left.Item()
                            .AlignCenter()
                            .Text("ИНВЕНТАРИЗАЦИОННАЯ ОПИСЬ (СЛИЧИТЕЛЬНАЯ ВЕДОМОСТЬ) № ____________")
                            .FontSize(11)
                            .Bold();

                        left.Item()
                            .AlignCenter()
                            .Text("по объектам нефинансовых активов")
                            .FontSize(7)
                            .Bold();

                        left.Item()
                            .PaddingTop(4)
                            .AlignCenter()
                            .Text("от «____» __________________ 20____ г.")
                            .FontSize(7);

                        left.Item()
                            .PaddingTop(8)
                            .Column(fields =>
                            {
                                fields.Item().Element(c => FormLine(c, "Учреждение"));
                                fields.Item().Element(c => FormLine(c, "Обособленное подразделение"));
                                fields.Item().Element(c => FormLine(c, "Структурное подразделение"));
                                fields.Item().Element(c => FormLine(c, "Тип имущества"));
                                fields.Item().Element(c => FormLine(c, "Наименование бюджета"));
                                fields.Item().Element(c => FormLine(c, "Единица измерения"));
                                fields.Item().Element(c => FormLine(c, "Ответственное лицо"));
                                fields.Item().Element(c => FormLine(c, "Решение о проведении инвентаризации"));
                                fields.Item().Element(c => FormLine(c, "Номер инвентаризационной комиссии"));
                                fields.Item().Element(c => FormLine(c, "Дата начала инвентаризации"));
                                fields.Item().Element(c => FormLine(c, "Дата окончания инвентаризации"));
                                fields.Item().Element(c => FormLine(c, "Дата, по состоянию на которую проводится инвентаризация"));
                            });
                    });

                    row.ConstantItem(150).Column(right =>
                    {
                        right.Item()
                            .AlignRight()
                            .Text("(в ред. Приказа Минфина России от 30.10.2023 № 174н)")
                            .FontSize(5);

                        right.Item()
                            .PaddingTop(4)
                            .Element(ComposeCodesBox);
                    });
                });
            });
        }

        private static void FormLine(IContainer container, string label)
        {
            container.PaddingBottom(2).Row(row =>
            {
                row.ConstantItem(215)
                    .AlignMiddle()
                    .Text(label)
                    .FontSize(7);

                row.RelativeItem()
                    .Height(13)
                    .BorderBottom(1)
                    .BorderColor(Colors.Black);
            });
        }

        private static void ComposeCodesBox(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.ConstantColumn(50);
                });

                table.Cell().ColumnSpan(2).Element(CodeCellStyle).AlignCenter().Text("КОДЫ").Bold();

                table.Cell().Element(CodeCellStyle).Text("Форма по ОКУД");
                table.Cell().Element(CodeCellStyle).Text("");

                table.Cell().Element(CodeCellStyle).Text("Дата");
                table.Cell().Element(CodeCellStyle).Text("");

                table.Cell().Element(CodeCellStyle).Text("по Сводному реестру");
                table.Cell().Element(CodeCellStyle).Text("");

                table.Cell().Element(CodeCellStyle).Text("Глава по БК");
                table.Cell().Element(CodeCellStyle).Text("");

                table.Cell().Element(CodeCellStyle).Text("по ОКТМО");
                table.Cell().Element(CodeCellStyle).Text("");

                table.Cell().Element(CodeCellStyle).Text("по ОКЕИ");
                table.Cell().Element(CodeCellStyle).Text("");
            });
        }

        private static IContainer CodeCellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Black)
                .PaddingVertical(2)
                .PaddingHorizontal(3)
                .MinHeight(14)
                .DefaultTextStyle(x => x.FontSize(5));
        }

        private static IContainer FormHeaderCellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Black)
                .PaddingVertical(3)
                .PaddingHorizontal(3)
                .DefaultTextStyle(x => x.Bold().FontSize(6))
                .AlignCenter()
                .AlignMiddle();
        }

        private static IContainer ColumnNumberCellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Black)
                .PaddingVertical(2)
                .PaddingHorizontal(3)
                .DefaultTextStyle(x => x.FontSize(6))
                .AlignCenter()
                .AlignMiddle();
        }

        private static IContainer FormBodyCellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Black)
                .PaddingVertical(3)
                .PaddingHorizontal(3)
                .DefaultTextStyle(x => x.FontSize(6))
                .AlignMiddle();
        }

        private static IContainer HeaderCellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Medium)
                .Background("#D0E3FF")
                .PaddingVertical(6)
                .PaddingHorizontal(5)
                .DefaultTextStyle(x => x.Bold().FontSize(10))
                .AlignMiddle();
        }

        private static IContainer BodyCellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten1)
                .PaddingVertical(5)
                .PaddingHorizontal(5)
                .DefaultTextStyle(x => x.FontSize(9))
                .AlignMiddle();
        }

        private static string GetInventoryPlace(InventoryReportItemViewModel item)
        {
            if (!string.IsNullOrWhiteSpace(item.Location) && !string.IsNullOrWhiteSpace(item.ClassroomNumber))
            {
                return $"{item.Location}, ауд. {item.ClassroomNumber}";
            }

            if (!string.IsNullOrWhiteSpace(item.Location))
            {
                return item.Location;
            }

            if (!string.IsNullOrWhiteSpace(item.ClassroomNumber))
            {
                return $"ауд. {item.ClassroomNumber}";
            }

            return string.Empty;
        }

        private static string FormatQuantity(decimal quantity)
        {
            return quantity % 1 == 0
                ? quantity.ToString("0")
                : quantity.ToString("0.##");
        }
    }
}