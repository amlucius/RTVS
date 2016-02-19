﻿using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Components.History {
    public interface IRHistoryProvider {
        IRHistory CreateRHistory(IRInteractiveWorkflow interactiveWorkflow);
        IRHistory GetAssociatedRHistory(ITextBuffer textBuffer);
        IRHistory GetAssociatedRHistory(ITextView textView);
        IRHistoryFiltering CreateFiltering(IRHistoryWindowVisualComponent visualComponent);
    }
}