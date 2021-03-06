﻿using System;
using System.Net;

namespace ChatApplication.Client
{
    [Serializable]
    public class ChatServiceException : Exception
    {
        public string ReasonPhrase { get; }
        public HttpStatusCode StatusCode { get; }

        public ChatServiceException(string message, string reasonPhrase, HttpStatusCode statusCode) : base(message)
        {
            ReasonPhrase = reasonPhrase;
            StatusCode = statusCode;
        }

        public ChatServiceException(string message, Exception e, string reasonPhrase, HttpStatusCode statusCode) : base(message, e)
        {
            ReasonPhrase = reasonPhrase;
            StatusCode = statusCode;
        }
    }
}