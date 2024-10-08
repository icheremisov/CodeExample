using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Client.Core.Common.Contracts
{
    public sealed class SharedMessage
    {
        public SharedMessageRoute Route { get; set; }

        public SharedError Error { get; set; }

        public byte[] Data { get; set; }

        public SharedRawData SharedRaw { get; set; }

        public int Timestamp { get; set; }

        public string PreHash { get; set; }

        public string PostHash { get; set; }

        public override string ToString() => $"{Route}";
    }

    public readonly struct SharedMessageRoute
    {
        public SharedVersion LogicVersion { get; }

        public string Service { get; }

        public string Method { get; }

        public bool IsValid => !string.IsNullOrEmpty(Service) && !string.IsNullOrEmpty(Method);

        public SharedMessageRoute(
            SharedVersion logicVersion,
            string service,
            string method)
        {
            LogicVersion = logicVersion;
            Service = service;
            Method = method;
        }

        public SharedMessageRoute(SharedMessageRoute route)
        {
            LogicVersion = route.LogicVersion;
            Service = route.Service;
            Method = route.Method;
        }

        public override string ToString() => $"{LogicVersion}.{Service}.{Method}";

        public string ToShortString() => Service + "." + Method;

        public bool IsServerError => Service == "Server" && Method == "Error";
    }

    public sealed class SharedVersion :
        IComparable,
        IComparable<SharedVersion>,
        IEquatable<SharedVersion>
    {
        public int Major { get; }

        public int Minor { get; }

        public int Revision { get; }

        public int Pipeline { get; }

        public SharedVersion()
        {
        }

        public SharedVersion(string str)
        {
            var sharedVersion = Parse(str);
            Major = sharedVersion.Major;
            Minor = sharedVersion.Minor;
            Revision = sharedVersion.Revision;
            Pipeline = sharedVersion.Pipeline;
        }

        public SharedVersion(int major, int minor, int revision, int pipeline)
        {
            if (major < 0)
                throw new ArgumentOutOfRangeException(nameof(major));
            if (minor < 0)
                throw new ArgumentOutOfRangeException(nameof(minor));
            if (revision < 0)
                throw new ArgumentOutOfRangeException(nameof(revision));
            if (pipeline < 0)
                throw new ArgumentOutOfRangeException(nameof(pipeline));
            Major = major;
            Minor = minor;
            Revision = revision;
            Pipeline = pipeline;
        }

        public override string ToString() =>
            Pipeline <= 0 ? $"{Major}.{Minor}.{Revision}" : $"{Major}.{Minor}.{Revision} #{Pipeline}";

        public int CompareTo(object obj)
        {
            var other = obj as SharedVersion;
            return (object)other == null ? -1 : CompareTo(other);
        }

        public bool Equals(SharedVersion other) => other != null && Major == other.Major && Minor == other.Minor &&
                                                   Revision == other.Revision && Pipeline == other.Pipeline;

        public int CompareTo(SharedVersion other)
        {
            if (other.Major != Major)
                return other.Major > Major ? -1 : 1;
            if (other.Minor != Minor)
                return other.Minor > Minor ? -1 : 1;
            return other.Revision != Revision
                ? (other.Revision > Revision ? -1 : 1)
                : (other.Pipeline == Pipeline ? 0 : (other.Pipeline > Pipeline ? -1 : 1));
        }

        public override bool Equals(object obj) =>
            obj != null && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((SharedVersion)obj));

        public override int GetHashCode() => HashCode.Combine(Major, Minor, Revision, Pipeline);

        public static SharedVersion Parse(string version, int pipeline) => Parse($"{version} #{pipeline}");

        public static SharedVersion Parse(string version)
        {
            if (string.IsNullOrEmpty(version))
                return null;
            var strArray = version.Split('.', ' ');
            if (strArray.Length < 2)
                ThrowInvalidFormatException(version);
            if (!int.TryParse(strArray[0].Trim(), out var major))
                ThrowInvalidFormatException(version);
            if (!int.TryParse(strArray[1].Trim(), out var minor))
                ThrowInvalidFormatException(version);
            var revision = 0;
            if (strArray.Length > 2 && !int.TryParse(strArray[2].Trim(), out revision))
                ThrowInvalidFormatException(version);
            var pipeline = 0;
            if (strArray.Length > 3)
            {
                if (!int.TryParse(strArray[3].TrimStart('#', ' ').TrimEnd(), out pipeline))
                    ThrowInvalidFormatException(version);
            }

            return new SharedVersion(major, minor, revision, pipeline);
        }

        public static SharedVersion FromAssembly(Assembly assembly)
        {
            var customAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return customAttribute == null ? null : Parse(customAttribute.InformationalVersion);
        }

        private static void ThrowInvalidFormatException(string version) =>
            throw new FormatException("Error parsing SharedVersion Major.Minor[.Revision] [#Pipeline] from '" +
                                      version + "'");

        public static bool operator ==(SharedVersion a, SharedVersion b) =>
            (object)a == null ? (object)b == null : a.Equals(b);

        public static bool operator !=(SharedVersion a, SharedVersion b) => !(a == b);

        public static bool operator >(SharedVersion a, SharedVersion b)
        {
            if (a == null)
                return false;
            if (b == null)
                return true;
            int num1 = a.Major;
            int num2 = b.Major;
            if (num1 == num2)
            {
                num1 = a.Minor;
                num2 = b.Minor;
            }

            if (num1 == num2)
            {
                num1 = a.Revision;
                num2 = b.Revision;
            }

            if (num1 == num2)
            {
                num1 = a.Pipeline;
                num2 = b.Pipeline;
            }

            return num1 > num2;
        }

        public static bool operator >=(SharedVersion a, SharedVersion b) => a > b || a.Equals(b);

        public static bool operator <(SharedVersion a, SharedVersion b) => b > a;

        public static bool operator <=(SharedVersion a, SharedVersion b) => b >= a;

        public bool IsCompatible(SharedVersion other) => !(other == null) && Major == other.Major &&
                                                         Minor >= other.Minor && Revision >= other.Revision &&
                                                         Pipeline >= other.Pipeline;
    }

    public sealed class SharedError
    {
        public string Message => Errors[0].Message;

        public string Type => Errors[0].Type;

        public string StackTrace => Errors[0].StackTrace;

        public ErrorEntry[] Errors { get; set; }

        public IEnumerable<ErrorEntry> InnerErrors => Errors.Skip(1);

        public ErrorCode Code { get; set; }

        public Dictionary<string, string> Params { get; set; } = new Dictionary<string, string>();

        public SharedError()
        {
        }

        public SharedError(ErrorCode code, ErrorEntry[] errors)
        {
            Code = code;
            Errors = errors;
        }

        public SharedError WithParams(string key, string value, bool overrideKey = false)
        {
            if (Params.ContainsKey(key) && !overrideKey)
                throw new Exception("Duplicated key " + key + " in exception: " + Type + ". Exists value " +
                                    Params[key] + ". Trying to set value " + value);
            Params[key] = value;
            return this;
        }

        public override string ToString() => $"{(object)Code} {Type} {Message}";

        public struct ErrorEntry
        {
            public string Type { get; set; }

            public string Message { get; set; }

            public string StackTrace { get; set; }

            public ErrorEntry(string type, string message, string stackTrace)
            {
                Type = type;
                Message = message;
                StackTrace = stackTrace;
            }

            public override string ToString() =>
                $"Type='{Type}' Message='{Message}'\nStackTrace: {StackTrace}";
        }
    }

    public readonly struct ErrorCode
    {
        public ErrorCode(string value) => Value = value;

        public bool Equals(ErrorCode other) => Value == other.Value;

        public override bool Equals(object obj) => obj is ErrorCode other && Equals(other);

        public override int GetHashCode() => Value == null ? 0 : Value.GetHashCode();

        public string Value { get; }

        public static bool operator ==(ErrorCode code1, ErrorCode code2) => code1.Value == code2.Value;

        public static bool operator !=(ErrorCode code1, ErrorCode code2) => code1.Value != code2.Value;

        public override string ToString() => Value;
    }
}