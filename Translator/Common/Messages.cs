using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translator
{
    public class TTargetChanged : ValueChangedMessage<string>
    {
        public TTargetChanged(string value) : base(value)
        {

        }
    }

    public class TProfileChanged : ValueChangedMessage<string>
    {
        public TProfileChanged(string value) : base(value)
        {

        }
    }

    public class TShuttingDown : ValueChangedMessage<string>
    {
        public TShuttingDown(string value) : base(value)
        {

        }
    }

    public class TAddProfileTestLogItem : ValueChangedMessage<TLogItem>
    {
        public TAddProfileTestLogItem(TLogItem value) : base(value)
        {

        }
    }

    public class TClearLog : ValueChangedMessage<bool>
    {
        public TClearLog(bool value) : base(value)
        {

        }
    }

    public class TTestProgress : ValueChangedMessage<int>
    {
        public TTestProgress(int value) : base(value)
        {

        }

    }
    public class TTranslateProgress : ValueChangedMessage<int>
    {
        public TTranslateProgress(int value) : base(value)
        {

        }
    }



}
