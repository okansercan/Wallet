using System;
using System.Data.SqlClient;
using HtmlAgilityPack;
using System.Text;

namespace Wallet.Batch
{
	public class CampaignLoader
	{
        private string connString;
        private static readonly string BonusUrl = "https://www.bonus.com.tr";
        private static readonly string WorldUrl = "https://www.worldcard.com.tr";
        private static readonly string MaximumUrl = "https://www.maximum.com.tr";

        public CampaignLoader(string connectionString)
		{
            connString = connectionString;
		}

        public void Load()
        {
            List<Campaign> campaigns = new List<Campaign>();
            campaigns.AddRange(GetBonusCampaigns());
            campaigns.AddRange(GetWorldCampaigns());
            campaigns.AddRange(GetMaximumCampaigns());

            string truncateSql = "Truncate Table Campaign";
            string insertSql = "Insert Into Campaign (Brand, Sector, Title, Description, ImageUrl, DetailUrl, Logo, LastDate) Values (@brand, @sector, @title, @desc, @imageUrl, @detailUrl, @logo, @lastDate)";

            using (var connection = new SqlConnection(connString))
            {
                connection.Open();

                //eski kampanya kayitlari silinir
                SqlCommand command = new SqlCommand(truncateSql, connection);
                command.ExecuteNonQuery();

                //yeni kampanya kayitlari eklenir
                foreach (var campaign in campaigns)
                {
                    command = new SqlCommand(insertSql, connection);

                    command.Parameters.Add("@brand", System.Data.SqlDbType.VarChar).Value = campaign.Brand;
                    command.Parameters.Add("@sector", System.Data.SqlDbType.VarChar).Value = campaign.Sector;
                    command.Parameters.Add("@title", System.Data.SqlDbType.VarChar).Value = campaign.Title;
                    command.Parameters.Add("@desc", System.Data.SqlDbType.VarChar).Value = campaign.Description == null ? string.Empty : campaign.Description;
                    command.Parameters.Add("@imageUrl", System.Data.SqlDbType.VarChar).Value = campaign.ImageUrl;
                    command.Parameters.Add("@detailUrl", System.Data.SqlDbType.VarChar).Value = campaign.DetailUrl;
                    command.Parameters.Add("@logo", System.Data.SqlDbType.VarChar).Value = campaign.Logo == null ? string.Empty : campaign.Logo;
                    command.Parameters.Add("@lastDate", System.Data.SqlDbType.VarChar).Value = campaign.LastDate == null ? string.Empty : campaign.LastDate;

                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        private static IEnumerable<Campaign> GetMaximumCampaigns()
        {
            List<Campaign> cmplist = new List<Campaign>();
            List<Sector> catList = new List<Sector>();
            string bankUrl = string.Format("{0}/kampanyalar", MaximumUrl);

            var web = new HtmlWeb();
            web.OverrideEncoding = Encoding.UTF8;
            var htmlDoc = web.Load(bankUrl);

            var catNodes = htmlDoc.DocumentNode.SelectNodes("//li[@catfid]");
            var cmpNodes = htmlDoc.DocumentNode.SelectNodes("//div[@campid]");

            //kampanya sektörleri aliniyor
            foreach (HtmlNode node in catNodes)
            {
                catList.Add(new Sector
                {
                    Id = string.Format("cat{0}", node.Attributes["catfid"].Value),
                    Name = node.ChildNodes.FirstOrDefault(c => c.Name == "a").ChildNodes.FirstOrDefault(c => c.Name == "div").InnerText
                });
            }

            //kampanyalar aliniyor
            foreach (HtmlNode node in cmpNodes)
            {
                var cmpContent = node.OuterHtml;
                var cmpDoc = new HtmlDocument();
                cmpDoc.LoadHtml(cmpContent);

                string ClassToGet = "card-text";
                string title = cmpDoc.DocumentNode.SelectSingleNode("//p[@class='" + ClassToGet + "']").InnerText;

                ClassToGet = "card-btn-group-2";
                var lnk = cmpDoc.DocumentNode.SelectSingleNode("//div[@class='" + ClassToGet + "']");
                string url = lnk.ChildNodes.FirstOrDefault(c => c.Name == "a").Attributes["href"].Value;
                url = string.Format("{0}{1}", MaximumUrl, url);
                string imageUrl = cmpDoc.DocumentNode.SelectSingleNode("//picture").SelectSingleNode("//source[@class='lazy-picture']").Attributes["srcset"].Value;
                imageUrl = string.Format("{0}{1}", MaximumUrl, imageUrl);
                string day = cmpDoc.DocumentNode.SelectNodes("//span").FirstOrDefault(c => c.OuterHtml.Contains("Time")).InnerText;
                string sector = string.Empty;
                var category = catList.FirstOrDefault(c => node.Attributes["class"].Value.Contains(c.Id));

                if (category != null)
                    sector = category.Name;

                cmplist.Add(new Campaign { Brand = "Maximum", Sector = sector, Title = title, LastDate = day, ImageUrl = imageUrl, DetailUrl = url });
            }

            return cmplist;
        }

        private static IEnumerable<Campaign> GetWorldCampaigns()
        {
            List<Sector> catlist = new List<Sector>();
            List<Campaign> cmplist = new List<Campaign>();
            string bankUrl = string.Format("{0}/kampanyalar", WorldUrl);

            var web = new HtmlWeb();
            web.OverrideEncoding = Encoding.UTF8;
            var htmlDoc = web.Load(bankUrl);

            string ClassToGet = "campaign-category";
            var node = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='" + ClassToGet + "']");

            //kampanya sektorleri aliniyor
            foreach (var catNode in node.ChildNodes.Where(c => c.Name == "div"))
            {
                var catItem = catNode.ChildNodes.FirstOrDefault(c => c.Name == "a");
                string catName = catItem.ChildNodes.FirstOrDefault(c => c.Name == "span").InnerText;
                string catId = catItem.Attributes["data-id"].Value;
                string catLink = catItem.Attributes["data-key"].Value;

                if (catId == "0")
                    continue;

                catlist.Add(new Sector
                {
                    Id = catId,
                    Link = catLink,
                    Name = catName
                });
            }

            //Sektorlere gore kampanyalar aliniyor
            foreach (var category in catlist)
            {
                cmplist.AddRange(GetWorldCampaigns(category));
            }

            return cmplist;
        }

        private static IEnumerable<Campaign> GetWorldCampaigns(Sector sector)
        {
            List<Campaign> cmplist = new List<Campaign>();
            string bankUrl = String.Format("{0}/kampanyalar/{1}", WorldUrl, sector.Link);

            var web = new HtmlWeb();
            web.OverrideEncoding = Encoding.UTF8;
            var htmlDoc = web.Load(bankUrl);

            string ClassToGet = "campaign-box";
            var node = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='" + ClassToGet + "']");

            foreach (var row in node.ChildNodes.Where(c => c.Name == "div"))
            {
                foreach (var cmp in row.ChildNodes.Where(c => c.Name == "div"))
                {
                    string title = cmp.ChildNodes.FirstOrDefault(c => c.Name == "p").InnerText;
                    string url = cmp.ChildNodes.FirstOrDefault(c => c.Name == "a").Attributes["href"].Value;
                    url = string.Format("{0}{1}", WorldUrl, url);
                    string day = cmp.ChildNodes.FirstOrDefault(c => c.Name == "div").ChildNodes.FirstOrDefault(c => c.Name == "p").InnerText;
                    string imageUrl = cmp.ChildNodes.FirstOrDefault(c => c.Name == "a").ChildNodes.FirstOrDefault(c => c.Name == "picture").ChildNodes.FirstOrDefault(c => c.Name == "img").Attributes["src"].Value;
                    imageUrl = string.Format("{0}{1}", WorldUrl, imageUrl);

                    cmplist.Add(new Campaign { Brand = "WorldCard", Sector = sector.Name, Title = title, ImageUrl = imageUrl, DetailUrl = url, LastDate = day });
                }
            }

            return cmplist;
        }

        private static IEnumerable<Campaign> GetBonusCampaigns()
        {
            List<Sector> catlist = new List<Sector>();
            List<Campaign> cmplist = new List<Campaign>();
            string bankUrl = string.Format("{0}/kampanyalar", BonusUrl);

            var web = new HtmlWeb();
            web.OverrideEncoding = Encoding.UTF8;
            var htmlDoc = web.Load(bankUrl);

            var cmpNodes = htmlDoc.DocumentNode.SelectNodes("//div[@data-sector]");

            string ClassToGet = "campaign-filter__select campaign-filter__select--sector";
            var catSelect = htmlDoc.DocumentNode.SelectSingleNode("//select[@class='" + ClassToGet + "']");

            //kampanya sektorleri aliniyor
            foreach (var catNode in catSelect.ChildNodes.Where(c => c.Name == "option"))
            {
                var catId = catNode.Attributes["value"].Value;
                var catName = catNode.InnerText;

                if (!catId.Equals("-1"))
                {
                    catlist.Add(new Sector
                    {
                        Id = catId.Replace("/sektor/", ""),
                        Name = catName
                    });
                }
            }

            foreach (HtmlNode node in cmpNodes)
            {
                var cmpContent = node.OuterHtml;
                var cmpDoc = new HtmlDocument();
                cmpDoc.LoadHtml(cmpContent);

                string title = node.Attributes["data-text"].Value;
                string sector = string.Empty;
                string url = node.ChildNodes.FirstOrDefault(c => c.Name == "a" && c.OuterHtml.Contains("href")).Attributes["href"].Value;
                url = string.Format("{0}{1}", BonusUrl, url);
                string imageUrl = cmpDoc.DocumentNode.SelectSingleNode("//div").SelectSingleNode("//img").Attributes["data-src"].Value;
                imageUrl = string.Format("{0}{1}", BonusUrl, imageUrl);
                var category = catlist.FirstOrDefault(c => node.Attributes["data-sector"].Value.Contains(c.Id));

                if (category != null)
                    sector = category.Name;

                cmplist.Add(new Campaign { Brand = "Bonus", Sector = sector, Title = title, ImageUrl = imageUrl, DetailUrl = url });
            }
            return cmplist;
        }
    }
}

