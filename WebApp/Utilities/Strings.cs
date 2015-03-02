using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace WebApp.Utilities
{
    public static class Strings
    {

        public static string GetTextConstant(string key, int languageId, string defaultValue = "")
        {
            var valueToReturn = "";
            using (var db = new DataContainer())
            {
                try
                {
                    var constant = GetTextConsts().FirstOrDefault(x => x.Key == key);
                    if (constant == null)
                    {
                        valueToReturn = CreateTextConstant(key, defaultValue, languageId, db);
                    }
                    else
                    {
                        var constantValue = constant.Values.Where(x => x.LangId == languageId).Select(x => x.Value).FirstOrDefault();
                        valueToReturn = string.IsNullOrWhiteSpace(constantValue) ? defaultValue : constantValue;
                    }
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }

            return valueToReturn;
        }

        private static string Key = "GetTextConsts";

        public static void ExpireTextConstants(HttpContext context = null)
        {
            var cache = context == null ? HttpContext.Current.Cache : context.Cache;
            cache.Remove(Key);
        }

        private static TextConst[] GetTextConsts(HttpContext context = null)
        {
            var cache = context == null ? HttpContext.Current.Cache : context.Cache;
            var data = cache.Get(Key) as TextConst[];

            if (data != null) return data;
            
            using (var db = new DataContainer())
            {
                try
                {
                    var tempList = new List<TextConst>() {};
                    var constants = db.TextConstants.ToArray();
                    tempList.AddRange(constants.Select(constant => new TextConst {Key = constant.TextConstName, Values = constant.TextConstantValues.Select(x => new TextConstValue {LangId = x.tConstValLangId, Value = x.tConstValValue}).ToArray()}));
                    data = tempList.ToArray();
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }
            cache.Add(Key, data, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal,null);
            return data;
        }

        public static string RemoveDiacritics(this string text)
        {
            if (text == null) return text;

            if (text.ToLower().Length > 0)
            {
                char[] chars = new char[text.Length];
                int charIndex = 0;

                text = text.Normalize(NormalizationForm.FormD);
                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                        chars[charIndex++] = c;
                }

                return new string(chars, 0, charIndex).Normalize(NormalizationForm.FormC);
            }

            return text;
        }

        private static string CreateTextConstant(string key, string value, int languageId, DataContainer db = null)
        {
            var database = db;
            if (db == null)
            {
                database = new DataContainer();
            }

            var textConstantGr = new TextConstantValues
            {
                tConstValLangId = 1,
                tConstValValue = value
            };

            var textConstantEn = new TextConstantValues
            {
                tConstValLangId = 2,
                tConstValValue = value
            };

            var textConstant = new TextConstants
            {
                TextConstDatetime = DateTime.Now,
                TextConstName = key,
            };

            textConstant.TextConstantValues.Add(textConstantGr);
            textConstant.TextConstantValues.Add(textConstantEn);

            database.TextConstants.Add(textConstant);
            database.SaveChanges();
            if (db == null)
            {
                database.Database.Connection.Close();
            }

            var textConstants = new TextConstantValues[] { textConstantGr, textConstantEn };

            return textConstants.Where(x => x.tConstValLangId == languageId).Select(x => x.tConstValValue).FirstOrDefault();

        }

    }

    public class TextConst
    {
        public string Key { get; set; }
        public TextConstValue[] Values { get; set; }
    }

    public class TextConstValue
    {
        public int LangId { get; set; }
        public string Value { get; set; }
    }
}