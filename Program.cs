using System;
using System.Threading;

namespace Simulator
{
    class Program
    {
        static void Main(string[] args)
        {
            int rows = int.Parse(args[0]);
            int cols = int.Parse(args[1]);
            int nThreads = int.Parse(args[2]);
            int nOp = int.Parse(args[3]);

            void WaitForThreads()
            {
                int maxThreads = 0;
                int placeHolder = 0;
                int availThreads = 0;
                int timeOutSeconds = 120;

                //Now wait until all threads from the Threadpool have returned
                while (timeOutSeconds > 0)
                {
                    //figure out what the max worker thread count it
                    System.Threading.ThreadPool.GetMaxThreads(out
                                         maxThreads, out placeHolder);
                    System.Threading.ThreadPool.GetAvailableThreads(out availThreads,
                                                                   out placeHolder);

                    if (availThreads == maxThreads) break;
                    // Sleep
                    System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(1000));
                    --timeOutSeconds;
                }
                // You can add logic here to log timeouts
            }
            SharableSpreadSheet SpreadSheet = new SharableSpreadSheet(rows, cols);
            SpreadSheet.setConcurrentSearchLimit(nThreads);
            SpreadSheet.nOperations = nOp;
            ThreadPool.SetMinThreads(1, 0);
            ThreadPool.SetMaxThreads(nThreads, 0);
            ThreadSearcher t = new ThreadSearcher(0, nOp, nThreads, SpreadSheet);
            //SpreadSheet.exchangeRows(1,4);
            //SpreadSheet.print();
            //t.RandomOperation(1);
            for (int i = 0; i < nThreads; i++)
            {
                ThreadPool.QueueUserWorkItem(t.RandomOperation, i);
                Thread.Sleep(100);

            }
            //return the loop after we finish debug the randomOperation function
            Thread.Sleep(1000);
            //Thread.CurrentThread.Join();
            WaitForThreads();


            SpreadSheet.save("spreadsheet.dat");

        }
    }


    class ThreadSearcher
    {
        int id;
        int nOperations;
        int nThreads;
        SharableSpreadSheet SharebleSpreadSheet;

        public ThreadSearcher(int id, int nOp, int nThread, SharableSpreadSheet SharebleSpreadSheet)
        {
            this.id = id;
            this.nOperations = nOp;
            this.nThreads = nThread;
            this.SharebleSpreadSheet = SharebleSpreadSheet;
        }
        public void RandomOperation(object state)
        {
            int name = (int)state;

            int num = this.SharebleSpreadSheet.nOperations;
            while (num > 0)
            {
                num--;
                var rand = new Random();
                int r = rand.Next(13);

                if (r == 0)
                {
                    int randRow = rand.Next(1, SharebleSpreadSheet.rows + 1);
                    int randCol = rand.Next(1, SharebleSpreadSheet.cols + 1);

                    String s = SharebleSpreadSheet.getCell(randRow, randCol);
                    Console.WriteLine("User[{0}]: string {1} found in cell [{2},{3}]", name, s, randRow, randCol);
                }

                if (r == 1)
                {
                    int randRow = rand.Next(1, SharebleSpreadSheet.rows + 1);
                    int randCol = rand.Next(1, SharebleSpreadSheet.cols + 1);
                    SharebleSpreadSheet.setCell(randRow, randCol, "***");
                    Console.WriteLine("User[{0}]: string *** updated cell [{1},{2}]", name, randRow, randCol);
                }

                if (r == 2)
                {
                    int randRow = rand.Next(1, SharebleSpreadSheet.rows + 1);
                    int randCol = rand.Next(1, SharebleSpreadSheet.cols + 1);

                    int row = 0;
                    int col = 0;

                    bool b = SharebleSpreadSheet.searchString("***", ref row, ref col);
                    if (b)
                    {
                        Console.WriteLine("User[{0}]: string *** found in cell [{1},{2}]", name, row, col);
                    }
                    else
                    {
                        Console.WriteLine("User[{0}]: string *** not found", name);
                    }

                }

                if (r == 3)
                {
                    int randRow1 = rand.Next(1, SharebleSpreadSheet.rows + 1);
                    int randRow2 = rand.Next(1, SharebleSpreadSheet.rows + 1);
                    while (randRow1 == randRow2)
                    {
                        randRow1 = rand.Next(1, SharebleSpreadSheet.rows + 1);
                    }

                    SharebleSpreadSheet.exchangeRows(randRow1, randRow2);
                    Console.WriteLine("User [{0}]: rows [{1}] and [{2}] exchanged successfully.", name, randRow1, randRow2);
                }

                if (r == 4)
                {
                    int randCol1 = rand.Next(1, SharebleSpreadSheet.cols + 1);
                    int randCol2 = rand.Next(1, SharebleSpreadSheet.cols + 1);
                    while (randCol1 == randCol2)
                    {
                        randCol1 = rand.Next(1, SharebleSpreadSheet.cols + 1);
                    }
                    SharebleSpreadSheet.exchangeCols(randCol1, randCol2);
                    Console.WriteLine("User [{0}]: columns [{1}] and [{2}] exchanged successfully.", name, randCol1, randCol2);
                }

                if (r == 5)
                {
                    int randRow = rand.Next(1, SharebleSpreadSheet.rows + 1);
                    int col = 0;
                    bool b = SharebleSpreadSheet.searchInRow(randRow, "***", ref col);
                    if (b)
                    {
                        Console.WriteLine("User[{0}]: string *** found in cell({1},{2})", name, randRow, col);
                    }
                    else
                    {
                        Console.WriteLine("User[{0}]: string *** not found in {1}", name, randRow);
                    }
                }

                if (r == 6)
                {
                    int randCol = rand.Next(1, SharebleSpreadSheet.cols + 1);
                    int row = 0;
                    bool b = SharebleSpreadSheet.searchInCol(randCol, "***", ref row);
                    if (b)
                    {
                        Console.WriteLine("User[{0}]: string *** found in cell({1},{2})", name, row, randCol);
                    }
                    else
                    {
                        Console.WriteLine("User[{0}]: string *** not found in {1}", name, randCol);
                    }
                }

                if (r == 7)
                {
                    int randRow1 = rand.Next(1, SharebleSpreadSheet.rows + 1);
                    int randRow2 = rand.Next(1, SharebleSpreadSheet.rows + 1);
                    int randCol1 = rand.Next(1, SharebleSpreadSheet.cols + 1);
                    int randCol2 = rand.Next(1, SharebleSpreadSheet.cols + 1);
                    int row = 0;
                    int col = 0;
                    bool b = SharebleSpreadSheet.searchInRange(randCol1, randCol2, randRow1, randRow2, "***", ref row, ref col);
                    if (b)
                    {
                        Console.WriteLine("User[{0}]: string *** found in cell({1},{2})", name, row, col);
                    }
                    else
                    {
                        Console.WriteLine("User[{0}]: string *** not found in the range ({1},{2}):({3}:{4})", name, randRow1, randCol1, randRow2, randCol2);
                    }
                }

                if (r == 8)
                {
                    int randRow = rand.Next(1, SharebleSpreadSheet.rows + 1);
                    SharebleSpreadSheet.addRow(randRow);
                    Console.WriteLine("User [{0}]: a new row added after row {1}", name, randRow);
                }

                if (r == 9)
                {
                    int randCol = rand.Next(1, SharebleSpreadSheet.cols + 1);
                    SharebleSpreadSheet.addCol(randCol);
                    Console.WriteLine("User [{0}]: a new column added after column {1}", name, randCol);
                }

                if (r == 10)
                {
                    int row = 0;
                    int col = 0;
                    SharebleSpreadSheet.getSize(ref row, ref col);
                    Console.WriteLine("User [{0}]: The size of the SpreadSheet is {1}x{2}", name, row, col);
                }

                if (r == 11)
                {
                    int randNumOfUsers = rand.Next(1, nThreads + 1);
                    SharebleSpreadSheet.setConcurrentSearchLimit(randNumOfUsers);
                    Console.WriteLine("User [{0}]: The number users limit is {1}", name, randNumOfUsers);
                }

                if (r == 12)
                {
                    SharebleSpreadSheet.save("spreadsheet.dat");
                    Console.WriteLine("User [{0}]: SpreadSheet saved", name);
                }



                Thread.Sleep(100);
            }

        }





    }
}