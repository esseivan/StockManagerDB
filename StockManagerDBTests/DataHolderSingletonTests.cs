﻿using ESNLib.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockManagerDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dhs = StockManagerDB.DataHolderSingleton;

namespace StockManagerDB.Tests
{
    [TestClass()]
    public class DataHolderSingletonTests
    {
        private string GetFile(int index)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ESN", "Defaults", "ut_" + index + ".smd");
            Console.WriteLine($"Path is : '{path}'");
            if (File.Exists(path))
                File.Delete(path);
            return path;
        }

        private dhs d => dhs.Instance;

        [TestMethod()]
        public void LoadNewTest()
        {
            string f = GetFile(1);
            dhs.LoadNew(f);

            Assert.IsNotNull(d);
            Assert.AreEqual(0, d.Parts.Count);
            Assert.AreEqual(0, d.Projects.Count);
        }

        [TestMethod()]
        public void LoadFileTest()
        {
            string f = GetFile(2);
            dhs.LoadNew(f);
            d.Load();

            Assert.IsNotNull(d);
            Assert.AreEqual(0, d.Parts.Count);
            Assert.AreEqual(0, d.Projects.Count);
        }

        [TestMethod()]
        public void CloseFileTest()
        {
            string f = GetFile(3);
            dhs.LoadNew(f);
            d.Load();

            Assert.IsNotNull(d);
            Assert.AreEqual(0, d.Parts.Count);
            Assert.AreEqual(0, d.Projects.Count);

            d.Close();

            Assert.IsNull(d);
        }
    }
}