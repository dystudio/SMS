using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sms.Entity;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Sms.WebAdmin.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
        {
            List<MemberCard> list = new List<MemberCard>();
            ConcurrentBag<MemberCard> bag = new ConcurrentBag<MemberCard>();
            ConcurrentQueue<MemberCard> queue = new ConcurrentQueue<MemberCard>();

            Stopwatch watch = new Stopwatch();
            int time = 10;
            int count = 1000;

            using (var db = new SmsEntities())
            {
                watch.Start();
                //for (int m = 0; m < time; m++)
                //{
                    for (int i = 0; i < count; i++)
                    {
                        list.Add(new MemberCard()
                        {
                            CardNo = "NO_" + i.ToString(),
                            Banlance = 0,
                            CreateTime = DateTime.Now,
                            Name = "Test_" + i.ToString(),
                            Status = 1
                        });
                    }
                //}
                db.MemberCard.AddRange(list);
                watch.Stop();
                Debug.WriteLine("单线程List执行{0}次完成，用时{1}毫秒", count, watch.ElapsedMilliseconds);
                Debug.WriteLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - -");

                watch.Restart();
                Parallel.For(0, count, (i) =>
                {
                    lock (list)
                    {
                        list.Add(new MemberCard()
                        {
                            CardNo = "NO_" + i.ToString(),
                            Banlance = 0,
                            CreateTime = DateTime.Now,
                            Name = "Test_" + i.ToString(),
                            Status = 1
                        });
                    }
                });
                db.MemberCard.AddRange(list);
                watch.Stop();
                Debug.WriteLine("直接锁住List执行{0}次完成，用时{1}毫秒", count, watch.ElapsedMilliseconds);
                Debug.WriteLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - -");

                watch.Restart();
                Parallel.For(0, count, (i) =>
                {
                    bag.Add(new MemberCard()
                    {
                        CardNo = "NO_" + i.ToString(),
                        Banlance = 0,
                        CreateTime = DateTime.Now,
                        Name = "Test_" + i.ToString(),
                        Status = 1
                    });
                });
                db.MemberCard.AddRange(bag);
                watch.Stop();
                Debug.WriteLine("ConcurrentBag执行{0}次完成，用时{1}毫秒", count, watch.ElapsedMilliseconds);
                Debug.WriteLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - -");

                watch.Restart();
                Parallel.For(0, count, (i) =>
                {
                    queue.Enqueue(new MemberCard()
                    {
                        CardNo = "NO_" + i.ToString(),
                        Banlance = 0,
                        CreateTime = DateTime.Now,
                        Name = "Test_" + i.ToString(),
                        Status = 1
                    });
                });
                db.MemberCard.AddRange(queue);
                watch.Stop();
                Debug.WriteLine("ConcurrentQueue执行{0}次完成，用时{1}毫秒", count, watch.ElapsedMilliseconds);
                Debug.WriteLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - -");
                Debug.WriteLine("  ");
                Debug.WriteLine("  ");
            }
            return View();
        }
    }
}