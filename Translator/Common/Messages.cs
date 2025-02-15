using CommunityToolkit.Mvvm.Messaging.Messages;

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

    public class TAddLogItem : ValueChangedMessage<TLogItem>
    {
        public TAddLogItem(TLogItem value) : base(value)
        {

        }
    }

    public class TSaveLog : ValueChangedMessage<TLog.eLogType>
    {
        public TSaveLog(TLog.eLogType value) : base(value)
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

    public class TCacheUdpated : ValueChangedMessage<bool>
    {
        public TCacheUdpated(bool value) : base(value)
        {

        }
    }

    public class TTargetLockChanged : ValueChangedMessage<bool>
    {
        public TTargetLockChanged(bool value) : base(value)
        {

        }
    }


}
