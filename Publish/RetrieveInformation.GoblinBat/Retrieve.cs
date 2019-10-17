﻿using System;
using System.Collections.Generic;
using System.IO;

namespace ShareInvest.RetrieveInformation
{
    public class Retrieve
    {
        protected List<string> ReadCSV(string file, List<string> list)
        {
            try
            {
                using (sr = new StreamReader(file))
                {
                    if (sr != null)
                        while (sr.EndOfStream == false)
                            list.Add(sr.ReadLine());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return list;
        }
        private StreamReader sr;
    }
}