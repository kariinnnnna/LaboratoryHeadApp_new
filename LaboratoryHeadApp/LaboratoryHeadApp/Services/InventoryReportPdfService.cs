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

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(column =>
                    {
                        column.Item().Text("Общий отчёт по оборудованию кафедры ИС")
                            .FontSize(18)
                            .Bold();

                        column.Item().PaddingTop(4)
                            .Text($"Дата формирования: {model.CreatedAt:dd.MM.yyyy HH:mm}");
                    });

                    page.Content().PaddingTop(12).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(28);
                            columns.ConstantColumn(95);
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(50);
                            columns.ConstantColumn(75);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCellStyle).Text("№");
                            header.Cell().Element(HeaderCellStyle).Text("Инв. номер");
                            header.Cell().Element(HeaderCellStyle).Text("Наименование");
                            header.Cell().Element(HeaderCellStyle).Text("Кол-во");
                            header.Cell().Element(HeaderCellStyle).Text("Аудитория");
                            header.Cell().Element(HeaderCellStyle).Text("МОЛ");
                            header.Cell().Element(HeaderCellStyle).Text("Описание");
                        });

                        for (int i = 0; i < model.Items.Count; i++)
                        {
                            var item = model.Items[i];

                            table.Cell().Element(BodyCellStyle).Text((i + 1).ToString());
                            table.Cell().Element(BodyCellStyle).Text(item.InventoryNumber ?? string.Empty);
                            table.Cell().Element(BodyCellStyle).Text(item.FullName ?? string.Empty);
                            table.Cell().Element(BodyCellStyle).Text(item.Quantity.ToString());
                            table.Cell().Element(BodyCellStyle).Text(
                                string.IsNullOrWhiteSpace(item.ClassroomNumber)
                                    ? "Не назначена"
                                    : item.ClassroomNumber);
                            table.Cell().Element(BodyCellStyle).Text(item.MaterialResponsiblePersonName ?? string.Empty);
                            table.Cell().Element(BodyCellStyle).Text(item.Description ?? string.Empty);
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
    }
}