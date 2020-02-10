//
// AuthzTagHelper.cs
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

using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FrontEnd.TagHelpers
{
    [HtmlTargetElement("*", Attributes = "authz")]
    [HtmlTargetElement("*", Attributes = "authz-policy")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AuthzTagHelper : TagHelper
    {
        private readonly IAuthorizationService _authzService;

        public AuthzTagHelper(IAuthorizationService authzService)
        {
            _authzService = authzService;
        }

        [HtmlAttributeName("authz")]
        public bool RequiresAuthentication { get; set; }

        [HtmlAttributeName("authz-policy")]
        public string RequiredPolicy { get; set; }

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var requiresAuth = RequiresAuthentication || !string.IsNullOrEmpty(RequiredPolicy);
            var showOutput = false;

            if (context.AllAttributes["authz"] != null && !requiresAuth && !ViewContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // authz="false" & user isn't authenticated
                showOutput = true;
            }
            else if (!string.IsNullOrEmpty(RequiredPolicy))
            {
                var cachedResult = ViewContext.ViewData["AuthPolicy." + RequiredPolicy];
                // authz-policy="foo" & user is authorized for policy "foo"
                bool authorized;
                if (cachedResult != null)
                {
                    authorized = (bool)cachedResult;
                }
                else
                {
                    var authResult = await _authzService.AuthorizeAsync(ViewContext.HttpContext.User, RequiredPolicy);
                    authorized = authResult.Succeeded;
                    ViewContext.ViewData["AuthPolicy." + RequiredPolicy] = authorized;
                }

                showOutput = authorized;
            }
            else if (requiresAuth && ViewContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // authz="true" & user is authenticated
                showOutput = true;
            }

            if (!showOutput)
            {
                output.SuppressOutput();
            }
        }
    }
}
