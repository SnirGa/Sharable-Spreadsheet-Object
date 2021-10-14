using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
/*
namespace Work3
{
    class ShareableSpreadSheet
    {
    }
}
using System;*/
class SharableSpreadSheet
{
    SemaphoreSlim saveAndLoadMutex;
    public int rows;
    public int cols;
    private String[,] grid;
    //addition
    private SemaphoreSlim[] rowMutexs; //mutex for every row- locked if there is a writer, else unlocked
    private SemaphoreSlim[] colMutexs; //mutex for every column-locked if there is a writer, else unlocked
    private SemaphoreSlim[] rowSemaphore = null; //semaphore for every row- locked if there is no more space for more readers in specific row, else unlocked 
    private SemaphoreSlim[] colSemaphore = null;//semaphore for every column- locked if there is no more space for more readers in specific column, else unlocked
    private SemaphoreSlim sema;
    SemaphoreSlim M = new SemaphoreSlim(1, 1);
    private int limit;
    public int nOperations;
    public SharableSpreadSheet(int nRows, int nCols)
    {
        // construct a nRows*nCols spreadsheet
        saveAndLoadMutex = new SemaphoreSlim(1, 1); //mutex
        rows = nRows;
        cols = nCols;
        grid = new String[nRows, nCols];
        for (int i = 0; i < nRows; i++)
        {
            for (int j = 0; j < nCols; j++)
            {
                int rowNum = i + 1;
                int colNum = j + 1;
                grid[i, j] = "testcell" + rowNum + colNum;
            }
        }
        rowMutexs = new SemaphoreSlim[rows]; //mutex
        colMutexs = new SemaphoreSlim[cols]; //mutex
        //addition 
        //sema = new Semaphore(limit, limit);
        rowSemaphore = null;
        colSemaphore = null;
        limit = -1;
        for (int i = 0; i < rowMutexs.Length; i++)
        {
            rowMutexs[i] = new SemaphoreSlim(1, 1); //initialize the rowMutexs array
        }
        colMutexs = new SemaphoreSlim[cols];
        for (int i = 0; i < colMutexs.Length; i++)
        {
            colMutexs[i] = new SemaphoreSlim(1, 1); //initialize the colMutexs array
        }



    }
    public String getCell(int row, int col)
    {
        row--;
        col--;
        if (row >= rows || col >= cols || row < 0|| col<0)
        {
             return null;
         }
        bool flag = false;
        rowMutexs[row].Wait();
        colMutexs[col].Wait();
        // String valueToReturn = grid[row, col];
        String valueToReturn = "";
        if (sema != null)
        {
            try
            {
                sema.Wait();
                valueToReturn = grid[row, col];
            }
            finally
            {

                sema.Release();
            }
        }
        else
        {
            valueToReturn = grid[row, col];
        }
        // return the string at [row,col] 
        rowMutexs[row].Release();
        colMutexs[col].Release();
        return valueToReturn; //atomic operation
        //return "";
    }
    public bool setCell(int row, int col, String str)
    {
        // if(row>rows || col > cols ||row<=0 ||col<=0)
        //    {
        //         return false;
        //     }
        row--;
        col--;
        rowMutexs[row].Wait();
        colMutexs[col].Wait();
        // set the string at [row,col]
        grid[row, col] = str; //atomic operation
        rowMutexs[row].Release();
        colMutexs[col].Release();
        return true;
    }
    public bool searchString(String str, ref int row, ref int col)
    {
        // search the cell with string str, and return true/false accordingly.
        // stores the location in row,col.
        // return the first cell that contains the string (search from first row to the last row)
        String temp;
        bool flag = false;
        for (int i = 1; i <= rows; i++)
        {
            for (int j = 1; j <= cols; j++)
            {
                temp = getCell(i, j);
                if (temp.Equals(str))
                {
                    row = i;
                    col = j;
                    return true;
                }
            }
        }
        return false;
    }


    public bool exchangeRows(int row1, int row2)
    {
        if (row1 == row2 || row1 > rows || row2 > rows || row1 <= 0 || row2 <= 0)
        {
            return false;
        }
        saveAndLoadMutex.Wait();
        // exchange the content of row1 and row2
        for (int i = 1; i <= cols; i++)
        {
            String rowCell1 = getCell(row1, i);
            String rowCell2 = getCell(row2, i);
            setCell(row1, i, rowCell2);
            setCell(row2, i, rowCell1);
        }
        saveAndLoadMutex.Release();
        return true;
    }
    public bool exchangeCols(int col1, int col2)
    {
        if (col1 == col2 || col1 > cols || col2 > cols)
        {
            return false;
        }
        // exchange the content of col1 and col2
        saveAndLoadMutex.Wait();
        for (int i = 1; i <= rows; i++)
        {
            String colCell1 = getCell(i, col1);
            String colCell2 = getCell(i, col2);
            setCell(i, col1, colCell2);
            setCell(i, col2, colCell1);

        }
        saveAndLoadMutex.Release();

        return true;
    }
    public bool searchInRow(int row, String str, ref int col)
    {
        if (row > rows || row <= 0)
        {
            return false;
        }
        // perform search in specific row

        for (int i = 1; i <= cols; i++)
        {
            String value = getCell(row, i);
            bool isEqual;
            if (value == null)
            {
                isEqual = false;
            }
            else
            {
                isEqual = value.Equals(str);
            }
            if (isEqual)
            {
                col = i;
                return true;
            }

        }
        return false;
    }
    public bool searchInCol(int col, String str, ref int row)
    {
        // perform search in specific col

        if (col > cols || col <= 0)
        {
            return false;
        }

        for (int i = 1; i <= rows; i++)
        {
            String value = getCell(i, col);
            bool isEqual;
            if (value == null)
            {
                isEqual = false;
            }
            else
            {
                isEqual = value.Equals(str);
            }
            if (isEqual)
            {
                row = i;
                return true;
            }
        }
        return false;
    }

    public bool searchInRange(int col1, int col2, int row1, int row2, String str, ref int row, ref int col)
    {
        if (col1 > cols || col2 > cols || col1 <= 0 || col2 <= 0 || row1 > rows || row2 > rows || row1 <= 0 || row2 <= 0)
        {
            return false;
        }
        // perform search within spesific range: [row1:row2,col1:col2] 
        //includes col1,col2,row1,row2
        for (int i = row1; i < row2 + 1; i++)
        {
            for (int j = col1; j < col2 + 1; j++)
            {
                if (getCell(i, j) == str)
                {
                    row = i;
                    col = j;
                    return true;
                }
            }
        }
        return true;
    }


    public bool addRow(int row1)
    {
        if (row1 <= 0 || row1 > rows)
        {
            return false;
        }
        saveAndLoadMutex.Wait();
        //add a row after row1
        String[,] temp = new String[rows + 1, cols];
        for (int i = 1; i <= row1; i++)
        {
            for (int j = 1; j <= cols; j++)
            {
                temp[i - 1, j - 1] = getCell(i, j);
            } // copy the "table" (2d array) to temp 2d array until the row we want add blank row inside it
        }

        for (int i = row1 + 1; i < rows + 1; i++)
        {
            for (int j = 1; j < cols + 1; j++)
            {
                temp[i, j - 1] = getCell(i, j);
            }
        }//insert the rest of the table after one blank row

        rows++;
        grid = temp;

        SemaphoreSlim[] temparray = new SemaphoreSlim[rowMutexs.Length + 1];
        for (int i = 0; i < row1; i++)
        {
            temparray[i] = rowMutexs[i];
        }
        temparray[row1] = new SemaphoreSlim(1, 1);
        for (int i = row1; i < rows - 1; i++)
        {
            temparray[i + 1] = rowMutexs[i];
        }
        //make the Mutex bigger by one
        rowMutexs = temparray;


        if (limit != -1)
        {
            /*
            Semaphore[] SemArray = new Semaphore[rowSemaphore.Length + 1];
            for (int i = 0; i < row1; i++)
            {
                SemArray[i] = rowSemaphore[i];
            }
            SemArray[row1] = new Semaphore(limit,limit);

            for (int i = row1; i < rows-1; i++)
            {
                SemArray[i+1] = rowSemaphore[i];
            }
            rowSemaphore = SemArray;
        */
        }
        for (int i = 1; i <= cols; i++)
        {
            setCell(row1 + 1, i, "testcell" + (row1 + 1) + i); //give default values to the blank row in the table
        }
        saveAndLoadMutex.Release();
        return true;

    }

    public bool addCol(int col1)
    {
        if (col1 > cols || col1 <= 0)
        {
            return false;
        }
        saveAndLoadMutex.Wait();
        if (col1 > cols)
        {
            return false;
        }
        //add a column after col1
        String[,] temp = new String[rows, cols + 1];
        for (int i = 1; i <= rows; i++)
        {
            for (int j = 1; j <= col1; j++)
            {
                temp[i - 1, j - 1] = getCell(i, j);
            }
        }
        for (int i = 1; i < rows + 1; i++)
        {
            for (int j = col1 + 1; j < cols + 1; j++)
            {
                temp[i - 1, j] = getCell(i, j);
            }
        }
        cols++;
        grid = temp;

        SemaphoreSlim[] temparray = new SemaphoreSlim[colMutexs.Length + 1];
        for (int i = 0; i < col1; i++)
        {
            temparray[i] = colMutexs[i];
        }
        temparray[col1] = new SemaphoreSlim(1, 1);
        for (int i = col1; i < cols - 1; i++)
        {
            temparray[i + 1] = colMutexs[i];
        }
        colMutexs = temparray;
        if (limit != -1)
        {/*
            Semaphore[] SemArray = new Semaphore[colSemaphore.Length + 1];
            for (int i = 0; i < col1; i++)
            {
                SemArray[i] = colSemaphore[i];
            }
            SemArray[col1] = new Semaphore(limit, limit);

            for (int i = col1; i < cols-1; i++)
            {
                SemArray[i+1] = colSemaphore[i];
            }
            colSemaphore = SemArray;
        */

        }
        for (int i = 1; i <= rows; i++)
        {
            setCell(i, col1 + 1, "testcell" + i + (col1 + 1));
        }
        saveAndLoadMutex.Release();
        return true;
    }
    public void getSize(ref int nRows, ref int nCols)
    {
        // return the size of the spreadsheet in nRows, nCols
        nRows = rows;
        nCols = cols;
    }
    public bool setConcurrentSearchLimit(int nUsers)
    {
        // this function aims to limit the number of users that can perform the search operations concurrently.
        // The default is no limit. When the function is called, the max number of concurrent search operations is set to nUsers. 
        // In this case additional search operations will wait for existing search to finish.
        limit = nUsers;

        sema = new SemaphoreSlim(1, limit);
        /* rowSemaphore = new Semaphore[rows];
         for(int i = 0; i < rows; i++)
         {
             rowSemaphore[i] = new Semaphore(limit, limit);
         }
         colSemaphore = new Semaphore[cols];
         for (int i = 0; i < cols; i++)
         {
             colSemaphore[i] = new Semaphore(limit, limit);

         }
        */
        return true;
    }

    public bool save(String fileName)
    {
        // save the spreadsheet to a file fileName.
        // you can decide the format you save the data. There are several options.
        saveAndLoadMutex.Wait();
        String str = "";
        str += rows;
        str += "|";
        str += cols;
        str += "|";

        for (int i = 1; i <= rows; i++)
        {
            for (int j = 1; j <= cols; j++)
            {
                String curr = getCell(i, j);
                if (curr.Equals(""))
                {
                    curr = "*";
                }
                str += curr;
                str += "|";
            }
        }
        using (StreamWriter file = File.CreateText(fileName))
        {
            file.WriteLine(str);
        }
        saveAndLoadMutex.Release();
        return true;
    }
    public bool load(String fileName)
    {
        // load the spreadsheet from fileName
        // replace the data and size of the current spreadsheet with the loaded data
        saveAndLoadMutex.Wait();
        String text;
        if (File.Exists(fileName))
        {
            text = File.ReadAllText(fileName);
        }
        else
        {
            text = "";
            return false;
        }

        bool afterRows = false;
        bool afterCols = false;
        String RowsstrRep = "";
        String ColStrRep = "";
        String str = "";
        int rows = -1;
        int cols = -1;
        int rowNum = 0;
        int colNum = 0;
        String[,] tempGrid = null;
        Char curr;


        for (int i = 0; i < text.Length; i++)
        {
            curr = text[i];
            if (!afterRows)
            {
                if (curr.Equals('|'))
                {
                    afterRows = true;
                }
                else
                {
                    RowsstrRep += curr;
                }
            }

            else if (!afterCols)
            {
                if (curr.Equals('|'))
                {
                    afterCols = true;
                    rows = int.Parse(RowsstrRep);
                    cols = int.Parse(ColStrRep);
                    tempGrid = new String[rows, cols];
                }

                else
                {
                    ColStrRep += curr;
                }
            }

            else if (afterRows && afterCols)
            {
                if (curr.Equals('|'))
                {
                    if (tempGrid != null)
                    {
                        tempGrid[rowNum, colNum] = str;
                        str = "";
                        if (colNum < cols - 1)
                        {
                            colNum++;
                        }

                        else
                        {
                            rowNum++;
                            colNum = 0;
                        }
                    }


                }

                else
                {
                    str += curr;
                }

            }
        }
        grid = tempGrid;
        saveAndLoadMutex.Release();
        return true;

    }
    public void print()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Console.Write(grid[i, j] + " ");
            }
            Console.Write("\n");
        }
    }

}
