﻿using System;
using ShareInvest.Catalog;
using ShareInvest.EventHandler;

namespace ShareInvest.XingAPI.Catalog
{
    internal class CFOBQ10500 : Query, IQuery
    {
        internal CFOBQ10500() : base()
        {

        }      
        protected override void OnReceiveData(string szTrCode)
        {
            var enumerable = GetOutBlocks();
            var temp = new string[enumerable.Count];

            while (enumerable.Count > 0)
            {
                var param = enumerable.Dequeue();

                for (int i = 0; i < GetBlockCount(param.Block); i++)
                    temp[temp.Length - enumerable.Count] = GetFieldData(param.Block, param.Field, i);
            }
            SendDeposit.Invoke(this, new Deposit(new string[]
            {
                temp[10],
                temp[11],
                temp[12],
                temp[19],
                temp[20],
                string.Empty,
                temp[21],
                temp[22],
                string.Empty,
                temp[23],
                temp[24],
                string.Empty,
                temp[28],
                temp[25],
                temp[26],
                temp[16],
                temp[17],
                temp[18],
                temp[13],
                temp[14],
                temp[29],
                string.Empty,
                temp[15],
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty
            }));
        }
        public void QueryExcute()
        {
            if (LoadFromResFile(new Secret().GetResFileName(GetType().Name)))
            {
                foreach (var param in GetInBlocks(GetType().Name))
                    SetFieldData(param.Block, param.Field, param.Occurs, param.Data);

                SendErrorMessage(Request(false));
            }
        }
        public event EventHandler<Deposit> SendDeposit;
    }
}