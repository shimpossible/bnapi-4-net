using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BNapi4Net;
using System.IO;

namespace BNapi4Net.Test
{
    class TestClient : BaseClient
    {

        public TestClient()
            : base()
        {
        }
        public string Test()            
        {
            Stream st = this.ReadData("d3/profile/femor-1367/hero/8321735");
            StreamReader sr = new StreamReader(st);
            return sr.ReadToEnd();
        }
    }

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            TestClient tc = new TestClient();
            string ret = tc.Test();
            Console.WriteLine(ret);
        }


        [TestMethod]
        public void GetProfile()
        {
            Diablo3.D3Client c = new Diablo3.D3Client();
            Diablo3.Profile p = c.GetProfile("femor-1367");
            p.Heroes[0].Refresh();

            Console.WriteLine(p.Heroes[0].Skills);
        }

        [TestMethod]
        public void GetHero()
        {
            Diablo3.ItemTypeId iti = Diablo3.ItemTypeId.Staff;

            Diablo3.D3Client c = new Diablo3.D3Client();
            Diablo3.Hero h = c.GetHero("femor-1367", 8321735);
        }

        [TestMethod]
        public void GetItem()
        {
            Diablo3.D3Client c = new Diablo3.D3Client();
            Diablo3.Item item = c.GetItem("COGHsoAIEgcIBBXIGEoRHYQRdRUdnWyzFB2qXu51MA04kwNAAFAKYJMD");
        }

        [TestMethod]
        public void GetFollower()
        {
            Diablo3.D3Client c = new Diablo3.D3Client();
            Diablo3.Artisan a = c.GetArtisan(Diablo3.ArtisanType.Blacksmith);
        }
    }
}
