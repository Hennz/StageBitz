using System;
using System.Collections;
using System.IO;
using System.Text;

namespace StageBitz.Common
{
    /// <summary>
    /// Class for read CSV files.
    /// </summary>
    public class CSVReader : IDisposable
    {
        private Stream objStream;
        private StreamReader objReader;

        /// <summary>
        /// Finalizes an instance of the <see cref="CSVReader"/> class.
        /// </summary>
        ~CSVReader()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (objStream != null)
                {
                    objStream.Dispose();
                    objStream = null;
                }
                if (objReader != null)
                {
                    objReader.Dispose();
                    objReader = null;
                }
            }
        }

        //add name space System.IO.Stream
        /// <summary>
        /// Initializes a new instance of the <see cref="CSVReader"/> class.
        /// </summary>
        /// <param name="filestream">The filestream.</param>
        public CSVReader(Stream filestream)
            : this(filestream, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CSVReader"/> class.
        /// </summary>
        /// <param name="filestream">The filestream.</param>
        /// <param name="enc">The enc.</param>
        public CSVReader(Stream filestream, Encoding enc)
        {
            this.objStream = filestream;
            //check the Pass Stream whether it is readable or not
            if (!filestream.CanRead)
            {
                return;
            }
            objReader = (enc != null) ? new StreamReader(filestream, enc) : new StreamReader(filestream);
        }

        //parse the Line
        /// <summary>
        /// Gets the CSV line.
        /// </summary>
        /// <returns></returns>
        public ArrayList GetCSVLine()
        {
            string data = objReader.ReadLine();
            if (data == null) return null;
            //if (data.Length == 0) return null;
            int position = -1;

            ArrayList result = new ArrayList();

            while (position < data.Length)
                result.Add(ParseCSVField(ref data, ref position));

            return result;
        }

        /// <summary>
        /// Parses the CSV field.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="StartSeperatorPos">The start seperator position.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">lines overflow:  + data</exception>
        private string ParseCSVField(ref string data, ref int StartSeperatorPos)
        {
            if (StartSeperatorPos == data.Length - 1)
            {
                StartSeperatorPos++;
                return "";
            }

            int fromPos = StartSeperatorPos + 1;
            if (data[fromPos] == '"')
            {
                int nextSingleQuote = GetSingleQuote(data, fromPos + 1);
                int lines = 1;
                while (nextSingleQuote == -1)
                {
                    data = data + "\n" + objReader.ReadLine();
                    nextSingleQuote = GetSingleQuote(data, fromPos + 1);
                    lines++;
                    if (lines > 20)
                        throw new Exception("lines overflow: " + data);
                }
                StartSeperatorPos = nextSingleQuote + 1;
                string tempString = data.Substring(fromPos + 1, nextSingleQuote - fromPos - 1);
                tempString = tempString.Replace("'", "''");
                return tempString.Replace("\"\"", "\"");
            }

            int nextComma = data.IndexOf(',', fromPos);
            if (nextComma == -1)
            {
                StartSeperatorPos = data.Length;
                return data.Substring(fromPos).Trim();
            }
            else
            {
                StartSeperatorPos = nextComma;
                return data.Substring(fromPos, nextComma - fromPos).Trim();
            }
        }

        /// <summary>
        /// Gets the single quote.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="SFrom">The s from.</param>
        /// <returns></returns>
        private int GetSingleQuote(string data, int SFrom)
        {
            int i = SFrom - 1;
            while (++i < data.Length)
                if (data[i] == '"')
                {
                    if (i < data.Length - 1 && data[i + 1] == '"')
                    {
                        i++;
                        continue;
                    }
                    else
                        return i;
                }
            return -1;
        }
    }
}