﻿using System;

[Serializable]
public enum MessageType
{
    Ack, Snapshot, Input, Join, JoinACK
}