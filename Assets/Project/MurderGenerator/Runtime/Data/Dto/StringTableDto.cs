using System;
using System.Collections.Generic;

// Mirror of strings_<language>.json: { "entries": [ { "group", "key", "value" } ] }.
namespace FifteenBraincells.MurderGenerator.Data.Dto
{
    [Serializable]
    public sealed class StringTableDto
    {
        public List<StringEntryDto> entries = new List<StringEntryDto>();
    }

    [Serializable]
    public sealed class StringEntryDto
    {
        public string group = "";
        public string key = "";
        public string value = "";
    }
}
