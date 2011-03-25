﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Machete.Domain;
using System.Diagnostics;
using Machete.Data;
using System.Data.Entity.Database;

namespace Machete.Test
{
    [TestClass]
    public class LINQ
    {
        [TestMethod]
        public void TestMethod1()
        {
            //Arrange
            var persons = new List<Person>();
            persons.Add(Records._person1);
            persons.Add(Records._person2);
            persons.Add(Records._person3);
            persons.Add(Records._person4);

            var workers = new List<Worker>();
            workers.Add(Records._worker1);
            workers.Add(Records._worker2);
            workers.Add(Records._worker3);

            var signins = new List<WorkerSignin>();
            signins.Add(new WorkerSignin { dwccardnum = 12345, dateforsignin = DateTime.Now });
            signins.Add(new WorkerSignin { dwccardnum = 12346, dateforsignin = DateTime.Now });
            signins.Add(new WorkerSignin { dwccardnum = 12347, dateforsignin = DateTime.Now }); 

            var q = from p in persons
                    join w in workers on p.ID equals w.ID into outer
                    from w in outer.DefaultIfEmpty()
                    select new
                    {
                        Name = p.firstname1,
                        height = ((w == null) ? "(no worker)" : w.height)
                    };

            foreach (var i in q)
            {
                Debug.WriteLine("Customer: {0}  Order Number: {1}",
                    i.Name.PadRight(11, ' '), i.height);
            }

            var q2 = from s in signins
                     join w in workers on s.dwccardnum equals w.dwccardnum into outer
                     from ps in outer.DefaultIfEmpty()
                     join p in persons on ps.ID equals p.ID
                     select new { s.dwccardnum, s.dateforsignin, ps.raceother, p.firstname1 };

            foreach (var ii in q2)
            {
            }
        }

        [TestMethod]
        public void TestMethod2()
        {
            //arrange
            DbDatabase.SetInitializer<MacheteContext>(new MacheteInitializer());
            MacheteContext DB = new MacheteContext();
            var q = from s in DB.WorkerSignins.AsEnumerable()
                    //join w in DB.Workers.AsEnumerable() on s.dwccardnum equals w.dwccardnum into outer
                    //from o in outer.DefaultIfEmpty()
                    //join p in DB.Persons.AsEnumerable() on o.ID equals p.ID
                    select s;
                   //select new WorkerSigninView()
                   //{
                   //    person = p,
                   //    signin = s      
                   //};
            foreach (var i in q)
            {
            }
        }

        [TestMethod]
        public void TestMethod3()
        {
            DbDatabase.SetInitializer<MacheteContext>(new MacheteInitializer());
            MacheteContext DB = new MacheteContext();
            DB.Database.Delete();
            DB.Database.Initialize(true);
            var q = from p in DB.Persons.AsEnumerable()
                    select p;
            foreach (var i in q)
            {
            }
        }
    }
}