using System;
using System.Collections.Generic;

namespace KeePassXC_API
{
    public class KeePassXCException : Exception
    {
        public static Dictionary<int, KeePassXCException> Exceptions = new()
        {
            { 0 , new KXCUnknowException() },
            { 1 , new KXCDatabaseNotOpenException() },
            { 2 , new KXCDatabaseHashNotRecivedException() },
            { 3 , new KXCPublicKeyNotRecivedException() },
            { 4 , new KXCCannotDecryptException() },
            { 5 , new KeePassXCException() },
            { 6 , new KXCActionDeniedException() },
            { 7 , new KXCPublicKeyNotRecivedException() },
            { 8 , new KXCAssotiationFailedException() },
            { 9 , new KXCKeyChangeFailedException() },
            { 10, new KXCEncryptionKeyNotRecognisedException() },
            { 11, new KXCNoSavedDatabaseFound() },
            { 12, new KXCIncorrectActionException()},
            { 13, new KXCEmptyMessageException()},
            { 14, new KeePassXCException()},
            { 15, new KXCNoLoginFoundException()}
        };
    }

    public class KXCUnknowException : KeePassXCException { }
    public class KXCDatabaseNotOpenException : KeePassXCException { }
    public class KXCDatabaseHashNotRecivedException : KeePassXCException { }
    public class KXCPublicKeyNotRecivedException : KeePassXCException { }
    public class KXCCannotDecryptException : KeePassXCException { }
    public class KXCActionDeniedException : KeePassXCException { }
    public class KXCAssotiationFailedException : KeePassXCException { }
    public class KXCKeyChangeFailedException : KeePassXCException { }
    public class KXCEncryptionKeyNotRecognisedException : KeePassXCException { }
    public class KXCNoSavedDatabaseFound : KeePassXCException { }
    public class KXCIncorrectActionException : KeePassXCException { }
    public class KXCEmptyMessageException : KeePassXCException { }
    public class KXCNoLoginFoundException : KeePassXCException { }
    public class KXCWrongMessageException : KeePassXCException { }
}
