using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raven.Database;
using Raven.Database.Config;
using Raven.Database.Data;
using Raven.Tests.Storage;
using Xunit;

namespace Raven.Tests.Queries
{
    public class DyanmicQueries : AbstractDocumentStorageTest
	{
		private readonly DocumentDatabase db;

		public DyanmicQueries()
		{
			db = new DocumentDatabase(new RavenConfiguration
				{
					DataDirectory = "raven.db.test.esent",
				});
		}

		public override void Dispose()
		{
			db.Dispose();
			base.Dispose();
		}

        [Fact]
        public void CanPerformQueryToSelectSingleItem()
        {
            db.Put("ayende", null, JObject.FromObject(new {Name = "Ayende"}), new JObject(), null);
            
            var result = db.ExecuteQueryUsingLinearSearch(new LinearQuery
            {
                Query = "from doc in docs select new { doc.Name }"
            });

            Assert.Empty(result.Errors);
            Assert.Equal(@"{""Name"":""Ayende"",""__document_id"":""ayende""}", result.Results[0].ToString(Formatting.None));
        }

        [Fact]
        public void CanSelectFullDocument()
        {
            db.Put("ayende", null, JObject.FromObject(new { Name = "Ayende" }), new JObject(), null);

            var result = db.ExecuteQueryUsingLinearSearch(new LinearQuery
            {
                Query = "from doc in docs select doc"
            });

            Assert.Empty(result.Errors);
            var jObject = result.Results[0];
            jObject.Remove("@metadata");
            Assert.Equal(@"{""Name"":""Ayende""}", jObject.ToString(Formatting.None));
        }


        [Fact]
        public void CanGetErrorsInQueries()
        {
            db.Put("ayende", null, JObject.FromObject(new { Name = "Ayende" }), new JObject(), null);
            db.Put("rahien", null, JObject.FromObject(new { Username = "Ayende" }), new JObject(), null);
            db.Put("oren", null, JObject.FromObject(new { Name = "Ayende" }), new JObject(), null);
           

            var result = db.ExecuteQueryUsingLinearSearch(new LinearQuery
            {
                Query = "from doc in docs select new { L = 1/doc.Name.Length } "
            });

            Assert.Equal("Doc 'rahien', Error: Operator '/' cannot be applied to operands of type 'int' and 'Raven.Database.Linq.DynamicNullObject'", result.Errors[0]);
            Assert.Equal(@"{""L"":0,""__document_id"":""ayende""}", result.Results[0].ToString(Formatting.None));
            Assert.Equal(@"{""L"":0,""__document_id"":""oren""}", result.Results[1].ToString(Formatting.None));
        }
	}
}
