using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Linq;

namespace Frequency2.REST.UrbanDictionary
{
	public class UrbanDictionaryClient
	{
		private const string Url = "http://api.urbandictionary.com/v0/define?term=";
		private async Task<dynamic> GetDynamicAsync(string term)
		{
			string uri = Url + term;
			string json;
			using (var webrequest = new HttpClient())
			{
				json = await webrequest.GetStringAsync(uri);
			}
			return JsonConvert.DeserializeObject<dynamic>(json);
		}

		public async Task<List<IDefinition>> GetDefinitionAsync(string term)
		{
			List<dynamic> dd = ((IEnumerable<dynamic>)(await GetDynamicAsync(term)).list).ToList();
			List<IDefinition> retVal = new List<IDefinition>();
			//Console.WriteLine(JsonConvert.SerializeObject(dd));
			foreach(var d in dd)
			{
				
				retVal.Add(new ADefinition
				{
					Definition = (string)d.definition,
					Thumbs_Up = (int)d.thumbs_up,
					Thumbs_Down = (int)d.thumbs_down,
					Author = (string)d.author,
					Word = (string)d.word,
					DefinitionId = (int)d.defid,
					Current_Vote = (string)d.current_vote,
					Written_On = (string)d.written_on,
					Example = (string)d.example
				});


			}

			return retVal;
		}

		private class ADefinition : IDefinition
		{
			public int Thumbs_Up { get; set; }

			public string Author { get; set; }

			public string Word { get; set; }

			public int DefinitionId { get; set; }

			public string Current_Vote { get; set; }

			public string Written_On { get; set; }

			public string Example { get; set; }

			public int Thumbs_Down { get; set; }

			public string Definition { get; set; }
		}
	}
}
