﻿#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Signum.Utilities;
using Signum.Entities;
using Signum.Web.Properties;
using Signum.Web.Controllers;
using Signum.Entities.Reflection;
#endregion

namespace Signum.Web
{
    public static class EntityBaseHelper
    {
        public static MvcHtmlString BaseLineLabel(HtmlHelper helper, BaseLine baseLine)
        {
            return BaseLineLabel(helper, baseLine, baseLine.Compose(EntityBaseKeys.ToStr));
        }

        public static MvcHtmlString BaseLineLabel(HtmlHelper helper, BaseLine baseLine, string idLabelFor)
        {
            return baseLine.LabelVisible && !baseLine.OnlyValue ?
                           helper.Label(baseLine.Compose("lbl"), baseLine.LabelText ?? "", idLabelFor, "sf-label-line") :
                           MvcHtmlString.Empty;
        }

        public static bool RequiresLoadAll(HtmlHelper helper, EntityBase eb)
        {
            return eb.IsNew == true;
        }

        public static MvcHtmlString RenderTypeContext(HtmlHelper helper, TypeContext typeContext, RenderMode mode, EntityBase line)
        {
            Type cleanRuntimeType = (typeContext.UntypedValue as Lite).TryCC(l => l.RuntimeType) ?? typeContext.UntypedValue.GetType();

            EntitySettings es = Navigator.Manager.EntitySettings.TryGetC(cleanRuntimeType)
                .ThrowIfNullC("There's no EntitySettings registered for type {0}".Formato(cleanRuntimeType));

            TypeContext tc = TypeContextUtilities.CleanTypeContext((TypeContext)typeContext);

            ViewDataDictionary vdd;
            if (line.PreserveViewData)
            {
                vdd = helper.ViewData;
                vdd.Model = tc;
            }
            else
            {
                vdd = new ViewDataDictionary(tc);
            }
            
            string partialViewName = line.PartialViewName;
            if (string.IsNullOrEmpty(partialViewName))
                partialViewName = es.OnPartialViewName((ModifiableEntity)tc.UntypedValue);

            switch (mode)
            {
                case RenderMode.Content:
                    return helper.Partial(partialViewName, vdd);
                case RenderMode.Popup:
                    vdd.Add(ViewDataKeys.PartialViewName, partialViewName);
                    return helper.Partial(Navigator.Manager.PopupControlView, vdd);
                case RenderMode.PopupInDiv:
                    vdd.Add(ViewDataKeys.PartialViewName, partialViewName);
                    return helper.Div(typeContext.Compose(EntityBaseKeys.Entity),
                        helper.Partial(Navigator.Manager.PopupControlView, vdd),
                        "",
                        new Dictionary<string, object> { { "style", "display:none" } });
                case RenderMode.ContentInVisibleDiv:
                case RenderMode.ContentInInvisibleDiv:
                    return helper.Div(typeContext.Compose(EntityBaseKeys.Entity),
                        helper.Partial(partialViewName, vdd), "",
                        (mode == RenderMode.ContentInInvisibleDiv) ? new Dictionary<string, object> { { "style", "display:none" } } : null);
                default:
                    throw new InvalidOperationException();
            }
        }

        public static string JsEscape(string input)
        {
            return input.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("/", "\\/").Replace("\r\n", "").Replace("\n", "");
        }

        public static MvcHtmlString ViewButton(HtmlHelper helper, EntityBase entityBase)
        {
            if (!entityBase.View)
                return MvcHtmlString.Empty;

            var htmlAttr = new Dictionary<string, object>
            {
                { "onclick", entityBase.GetViewing() },
                { "data-icon", "ui-icon-circle-arrow-e" },
                { "data-text", false}
            };

            if (entityBase.UntypedValue == null)
                htmlAttr.Add("style", "display:none");

            return helper.Href(entityBase.Compose("btnView"),
                  Resources.LineButton_View,
                  "",
                  Resources.LineButton_View,
                  "sf-line-button sf-view",
                  htmlAttr);
        }

        public static MvcHtmlString CreateButton(HtmlHelper helper, EntityBase entityBase)
        {
            if (!entityBase.Create)
                return MvcHtmlString.Empty;

            var htmlAttr = new Dictionary<string, object>
            {
                { "onclick", entityBase.GetCreating() },
                { "data-icon", "ui-icon-circle-plus" },
                { "data-text", false}
            };

            if (entityBase.UntypedValue != null)
                htmlAttr.Add("style", "display:none");

            return helper.Href(entityBase.Compose("btnCreate"),
                  Resources.LineButton_Create,
                  "",
                  Resources.LineButton_Create,
                  "sf-line-button sf-create",
                  htmlAttr);
        }

        public static MvcHtmlString FindButton(HtmlHelper helper, EntityBase entityBase)
        {
            if (!entityBase.Find || !entityBase.Type.CleanType().IsIIdentifiable())
                return MvcHtmlString.Empty;

            var htmlAttr = new Dictionary<string, object>
            {
                { "onclick", entityBase.GetFinding() },
                { "data-icon", "ui-icon-circle-zoomin" },
                { "data-text", false}
            };

            if (entityBase.UntypedValue != null)
                htmlAttr.Add("style", "display:none");

            return helper.Href(entityBase.Compose("btnFind"),
                  Resources.LineButton_Find,
                  "",
                  Resources.LineButton_Find,
                  "sf-line-button sf-find",
                  htmlAttr);
        }

        public static MvcHtmlString RemoveButton(HtmlHelper helper, EntityBase entityBase)
        {
            if (!entityBase.Remove)
                return MvcHtmlString.Empty;

            var htmlAttr = new Dictionary<string, object>
            {
                { "onclick", entityBase.GetRemoving() },
                { "data-icon", "ui-icon-circle-close" },
                { "data-text", false}
            };

            if (entityBase.UntypedValue == null)
                htmlAttr.Add("style", "display:none");

            return helper.Href(entityBase.Compose("btnRemove"),
                  Resources.LineButton_Remove,
                  "",
                  Resources.LineButton_Remove,
                  "sf-line-button sf-remove",
                  htmlAttr);
        }

        internal static MvcHtmlString EmbeddedTemplate(EntityBase entityBase, MvcHtmlString template)
        {
            return MvcHtmlString.Create("<script type=\"text/javascript\">var {0} = \"{1}\"</script>".Formato(
                                entityBase.Compose(EntityBaseKeys.Template),
                                EntityBaseHelper.JsEscape(template.ToHtmlString())));
        }

        public static void ConfigureEntityBase(EntityBase eb, Type entityType)
        {
            Common.TaskSetImplementations(eb);

            ConfigureEntityButtons(eb, entityType);
        }

        public static void ConfigureEntityButtons(EntityBase eb, Type entityType)
        {
            if (eb.Implementations == null && Navigator.Manager.EntitySettings.ContainsKey(entityType))
            {
                eb.Create = Navigator.IsCreable(entityType, false);
                eb.View = Navigator.IsViewable(entityType, false);
                eb.Find = Navigator.IsFindable(entityType);

                EntityLine el = eb as EntityLine;
                if (el != null)
                    el.Navigate = Navigator.IsNavigable(entityType, false);
            }
        }
    }

    public enum RenderMode
    {
        Popup,
        PopupInDiv,
        Content,
        ContentInVisibleDiv,
        ContentInInvisibleDiv
    }   
}
