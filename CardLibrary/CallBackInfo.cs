﻿/*
 * Program:         CardsLibrary.dll
 * Module:          CallbackInfo.cs
 * Date:            March 22, 2019
 * Author:          T. Haworth
 * Description:     The Card class represents a WCF data contract for sending
 *                  realtime updates to connected clients regarding changes to the 
 *                  state of the Shoe (service object).
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization; // WCF DataContract types

namespace CardLibrary
{
    [DataContract]
    public class CallBackInfo 
    {
        [DataMember] public int NumCards { get; private set; }
        [DataMember] public bool EmptyHand { get; private set; }

        public CallBackInfo(int c, bool e)
        {
            NumCards = c;
            EmptyHand = e;
        }
    }
}
