﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ShareInvest.EventHandler;
using ShareInvest.Interface;
using ShareInvest.Interface.XingAPI;

namespace ShareInvest.XingAPI.Catalog
{
    class T8411 : Query, ICharts<SendSecuritiesAPI>
    {
        public void QueryExcute(IRetention retention)
        {
            if (LoadFromResFile(Secrecy.GetResFileName(GetType().Name)))
            {
                InBlock = new HashSet<InBlock>();
                Milliseconds = 0x3ED;
                SendMessage(retention.Code, retention.LastDate, string.Empty);

                foreach (var param in GetInBlocks(GetType().Name))
                    if (InBlock.Add(new InBlock
                    {
                        Block = param.Block,
                        Field = param.Field,
                        Occurs = param.Occurs,
                        Data = param.Data ?? retention.Code
                    }))
                        SetFieldData(param.Block, param.Field, param.Occurs, param.Data ?? retention.Code);

                new Task(() =>
                {
                    Thread.Sleep(Milliseconds);
                    SendErrorMessage(GetType().Name, Request(false));
                }).Start();
            }
            Charts = new Stack<string>();
            Retention = retention.LastDate?.Substring(0, 12);
        }
        protected internal override void OnReceiveData(string szTrCode)
        {
            var enumerable = GetOutBlocks();
            var list = new List<string[]>();
            var index = int.MinValue;
            var code = string.Empty;

            while (enumerable.Count > 0)
            {
                var param = enumerable.Dequeue();

                switch (enumerable.Count)
                {
                    case int count when count > 0x1E:
                        var block = InBlock.First(o => o.Field.Equals(param.Field));
                        SetFieldData(block.Block, block.Field, block.Occurs, block.Data);
                        continue;

                    case int count when count < 9 || count == 9 && Decompress(param.Block) > 0:
                        var bCount = GetBlockCount(param.Block);
                        var array = new string[bCount];

                        for (int i = 0; i < bCount; i++)
                            array[i] = GetFieldData(param.Block, param.Field, i);

                        list.Add(array);
                        break;

                    case int count when count == 0xF || count == 0xE:
                        var data = GetFieldData(param.Block, param.Field, 0);
                        var refresh = InBlock.First(o => o.Field.Equals(param.Field));
                        SetFieldData(refresh.Block, refresh.Field, refresh.Occurs, data);
                        continue;

                    case 0x1C:
                        var temp = InBlock.First(o => o.Field.Equals(param.Field));
                        SetFieldData(temp.Block, temp.Field, temp.Occurs, temp.Data);
                        continue;

                    case 0x1B:
                        code = GetFieldData(param.Block, param.Field, 0);
                        continue;

                    case 0xA:
                        if (int.TryParse(GetFieldData(param.Block, param.Field, 0), out int rCount))
                            index = rCount;

                        continue;
                }
            }
            var span = WaitForTheLimitTime(GetTRCountRequest(szTrCode));
            SendMessage(span);

            if (span.TotalSeconds > 0xC4 && span.TotalSeconds < 0xC7)
                Milliseconds = (int)span.TotalMilliseconds;

            else
                Milliseconds = 0x3ED / GetTRCountPerSec(szTrCode);

            for (int i = index - 1; i >= 0; i--)
                if (uint.TryParse(list[1][i].Substring(0, 4), out uint time) && time > 0x35B && time < 0x604)
                {
                    var temp = string.Concat(list[0][i].Substring(2), list[1][i].Substring(0, 6));

                    if (string.IsNullOrEmpty(Retention) || string.Compare(temp, Retention) > 0)
                        Charts.Push(string.Concat(temp, ";", list[5][i], ";", list[6][i]));

                    else
                    {
                        SendMessage(code, temp, Retention);
                        Send?.Invoke(this, new SendSecuritiesAPI(code, Charts));

                        return;
                    }
                }
            if (IsNext)
                new Task(() =>
                {
                    Thread.Sleep(Milliseconds);
                    SendErrorMessage(GetType().Name, Request(IsNext));
                }).Start();
            else
                Send?.Invoke(this, new SendSecuritiesAPI(code, Charts));
        }
        protected internal override void OnReceiveMessage(bool bIsSystemError, string nMessageCode, string szMessage) => base.OnReceiveMessage(bIsSystemError, nMessageCode, szMessage);
        HashSet<InBlock> InBlock
        {
            get; set;
        }
        Stack<string> Charts
        {
            get; set;
        }
        string Retention
        {
            get; set;
        }
        int Milliseconds
        {
            get; set;
        }
        public event EventHandler<SendSecuritiesAPI> Send;
    }
}