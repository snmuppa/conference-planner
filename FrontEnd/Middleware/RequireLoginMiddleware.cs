//
// RequireLoginMiddleware.cs
//
// Author:
//       Sai Muppa <snmuppa@gmail.com>
//
// Copyright (c) 2020 (c) Sai Muppa
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FrontEnd.Middleware
{
    public class RequireLoginMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly LinkGenerator _linkGenerator;

        public RequireLoginMiddleware(RequestDelegate next, LinkGenerator linkGenerator)
        {
            _next = next;
            _linkGenerator = linkGenerator;
        }

        public Task Invoke(HttpContext context)
        {
            var endpoint = context.GetEndpoint();

            // If the user is authenticated but not a known attendee *and* we've not marked this page
            // to skip attendee welcome, then redirect to the Welcome page
            if (context.User.Identity.IsAuthenticated &&
                endpoint?.Metadata.GetMetadata<SkipWelcomeAttribute>() == null)
            {
                var isAttendee = context.User.IsAttendee();

                if (!isAttendee)
                {
                    var url = _linkGenerator.GetUriByPage(context, page: "/Welcome");
                    // No attendee registerd for this user
                    context.Response.Redirect(url);

                    return Task.CompletedTask;
                }
            }

            return _next(context);
        }
    }
}
