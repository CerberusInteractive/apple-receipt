﻿using System;
using System.IO;
using System.Text;
using Apple.Receipt.Parser.Asn1;

namespace Apple.Receipt.Parser.Services.NodesParser
{
    internal class Asn1NodesParser : IAsn1NodesParser
    {
        private readonly IAsn1ParserUtilitiesService _utilities;

        public Asn1NodesParser(IAsn1ParserUtilitiesService utilities)
        {
            _utilities = utilities;
        }

        public bool GetBoolFromNode(Asn1Node nn)
        {
            string stringFromNode = GetStringFromNode(nn);
            bool result;
            return bool.TryParse(stringFromNode, out result) && result;
        }

        public string GetStringFromNode(Asn1Node nn)
        {
            string dataStr = null;

            if ((nn.Tag & Asn1Type.Date) == Asn1Type.OctetString && nn.ChildNodeCount > 0)
            {
                Asn1Node n = nn.GetChildNode(0);

                switch (n.Tag & Asn1Type.Date)
                {
                    case Asn1Type.PrintableString:
                    case Asn1Type.Ia5String:
                    case Asn1Type.UniversalString:
                    case Asn1Type.VisibleString:
                    case Asn1Type.NumericString:
                    case Asn1Type.UtcTime:
                    case Asn1Type.Utf8String:
                    case Asn1Type.Bmpstring:
                    case Asn1Type.GeneralString:
                    case Asn1Type.GeneralizedTime:
                    {
                        if ((n.Tag & Asn1Type.Date) == Asn1Type.Utf8String)
                        {
                            UTF8Encoding unicode = new UTF8Encoding();
                            dataStr = unicode.GetString(n.Data);
                        }
                        else
                        {
                            dataStr = Encoding.ASCII.GetString(n.Data);
                        }
                    }
                        break;
                }
            }
            return dataStr;
        }

        public DateTime GetDateTimeFromNode(Asn1Node nn)
        {
            string dataStr = GetStringFromNode(nn);
            if (string.IsNullOrEmpty(dataStr))
            {
                return DateTime.MinValue;
            }
            DateTime retval = DateTime.MaxValue;
            DateTime.TryParse(dataStr, out retval);
            return retval;
        }

        public string GetDateTimeMsFromNode(Asn1Node nn)
        {
            DateTime date = GetDateTimeFromNode(nn);
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalMilliseconds).ToString();
        }

        public int GetIntegerFromNode(Asn1Node nn)
        {
            int retval = -1;

            if ((nn.Tag & Asn1Type.Date) == Asn1Type.OctetString && nn.ChildNodeCount > 0)
            {
                Asn1Node n = nn.GetChildNode(0);
                if ((n.Tag & Asn1Type.Date) == Asn1Type.Integer)
                {
                    retval = (int) _utilities.BytesToLong(n.Data);
                }
            }
            return retval;
        }

        public Asn1Node GetNodeFromBytes(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                Asn1Node resultNode = new Asn1Node();
                try
                {
                    resultNode.RequireRecalculatePar = false;
                    resultNode.InternalLoadData(stream);
                    return resultNode;
                }
                finally
                {
                    resultNode.RequireRecalculatePar = true;
                    resultNode.RecalculateTreePar();
                }
            }
        }
    }
}