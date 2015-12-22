﻿using System;
using Exceptionless.Dependency;
using Exceptionless.Services;

namespace Exceptionless.Plugins {
    [Priority(100)]
    public class SessionManagerPlugin : IEventPlugin {
        public void Run(EventPluginContext context) {
            // only manage session ids if the session id isn't specified manually
            if (!String.IsNullOrEmpty(context.Event.SessionId))
                return;

            var sessionManager = context.Resolver.Resolve<ISessionManager>();

            string identity = context.Event.GetUserIdentity()?.Identity ?? context.Event.GetRequestInfo()?.ClientIpAddress;
            if (String.IsNullOrEmpty(identity))
                return;

            string sessionId = null;

            if (context.Event.IsSessionStart()) {
                sessionId = sessionManager.StartSession(identity);
            } else if (context.Event.IsSessionEnd()) {
                sessionId = sessionManager.GetSessionId(identity);
                sessionManager.EndSession(sessionId);
            } else {
                sessionId = sessionManager.GetSessionId(identity);
                if (String.IsNullOrEmpty(sessionId)) {
                    sessionId = sessionManager.StartSession(identity);
                    context.Client.CreateSessionStart(sessionId).;
                }
            }

            context.Event.SessionId = sessionId;
        }
    }
}