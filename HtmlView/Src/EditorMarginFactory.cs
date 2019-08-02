using System;
using System.Collections;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace HtmlView
{
	[Export(typeof(IWpfTextViewMarginProvider))]
	[Name("HtmlViewMargin")]
	[Order(After = PredefinedMarginNames.VerticalScrollBarContainer)]
	[MarginContainer(PredefinedMarginNames.Right)]
	[ContentType("htmlx")]
	[TextViewRole(PredefinedTextViewRoles.Interactive)]
	[TextViewRole(PredefinedTextViewRoles.Document)]
	[TextViewRole(PredefinedTextViewRoles.Editable)]
	internal sealed class EditorMarginFactory : IWpfTextViewMarginProvider
	{
		[Import]
		internal ITextDocumentFactoryService TextDocumentFactoryService = null;

		public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
		{
			ITextDocument document;
			if(!TextDocumentFactoryService.TryGetTextDocument(wpfTextViewHost.TextView.TextDataModel.DocumentBuffer, out document))
				return null;

			return new EditorMargin(wpfTextViewHost.TextView, document);
		}
	}
}