using UnityEngine;
using System;

namespace GoogleSheets.Samples
{
    [CreateAssetMenu(fileName = "LocalizationConfig", menuName = "Configs/GoogleSheets/Samples/LocalizationConfig", order = 1)]
    public class LocalizationConfig : SheetData
    {
        [Serializable]
        public class Item
        {
            public string id;
            public Language[] languages;

            public Item(string id, Language[] languages)
            {
                this.id = id;
                this.languages = languages;
            }
        }

        [Serializable]
        public class Language
        {
            public string languageId;
            public string value;

            public Language(string languageId, string value)
            {
                this.languageId = languageId;
                this.value = value;
            }
        }

        [SerializeField] private Item[] _items;

        protected override void UpdateSheetData()
        {
            var items = String.GetArrayVertical("item");
            var codes = String.GetArrayHorizontal("item");

            _items = new Item[items.Length];
            for (int i = 0; i < _items.Length; i++)
            {
                _items[i] = new Item(items[i], ParseLanguages(codes, items[i]));
            }
        }

        private Language[] ParseLanguages(string[] codes, string item)
        {
            var languages = String.GetArrayHorizontal(item);
            var result = new Language[codes.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new Language(codes[i], languages[i]);

                if (string.IsNullOrWhiteSpace(languages[i]))
                {
                    Debug.LogWarning($"'{item}' - '{codes[i]}' has empty value");
                }
            }

            return result;
        }

        public string GetText(string id, string language)
        {
            foreach (var item in _items)
            {
                if (item.id == id)
                {
                    foreach (var lang in item.languages)
                    {
                        if (lang.languageId == language)
                        {
                            return lang.value;
                        }
                    }

                    return item.languages[0].value;
                }
            }

            Debug.LogError($"Not exist localization for '{id}'");
            return id;
        }

        public Item GetItem(string id)
        {
            foreach (var item in _items)
            {
                if (item.id == id)
                {
                    return item;
                }
            }

            Debug.LogError($"Not exist item '{id}'");
            return null;
        }
    }
}
