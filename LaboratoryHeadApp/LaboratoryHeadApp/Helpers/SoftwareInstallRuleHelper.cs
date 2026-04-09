using MolServiceContracts.ViewModels;

namespace LaboratoryHeadApp.Helpers
{
    public static class SoftwareInstallRuleHelper
    {
        private static readonly string[] AllowKeywords =
        {
            "компьютер", "пк", "пэвм", "ноутбук", "моноблок",
            "сервер", "терминал", "роутер", "маршрутизатор",
            "коммутатор", "точка доступа", "принтер", "мфу", "сканер"
        };

        private static readonly string[] DenyKeywords =
        {
            "стол", "шкаф", "кровать", "жалюзи", "кондиционер",
            "тумба", "диван", "стеллаж", "кресло", "баннер",
            "патент", "свидетельство", "программное обеспечение"
        };

        public static bool CanInstallSoftware(MaterialTechnicalValueViewModel model)
        {
            var name = Normalize(model.FullName);
            var description = Normalize(model.Description);

            if (ContainsAny(name, DenyKeywords) || ContainsAny(description, DenyKeywords))
            {
                return false;
            }

            if (ContainsAny(name, AllowKeywords) || ContainsAny(description, AllowKeywords))
            {
                return true;
            }

            return false;
        }

        public static string GetRestrictionReason(MaterialTechnicalValueViewModel model)
        {
            if (model.Quantity <= 0)
            {
                return "Оборудование полностью списано.";
            }

            return "Для данного типа оборудования установка ПО не предусмотрена.";
        }

        private static string Normalize(string? value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static bool ContainsAny(string source, IEnumerable<string> keywords)
        {
            return keywords.Any(k => source.Contains(k));
        }
    }
}
