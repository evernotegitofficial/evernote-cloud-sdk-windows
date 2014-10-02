using System;
using System.Runtime.Serialization;
using System.Security;

namespace EvernoteSDK
{
	[Serializable()]
	public class ENUnknownException : System.Exception
	{
		/// <summary>
		/// Initializes a new instance of the class.
		/// </summary>
		public ENUnknownException() : base()
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public ENUnknownException(string message) : base(message)
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the
		/// current exception, or a null reference if no inner exception is
		/// specified</param>
		public ENUnknownException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		// Constructor required for serialization
		[SecuritySafeCritical()]
		protected ENUnknownException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

	}

	[Serializable()]
	public class ENConnectionFailedException : System.Exception
	{
		/// <summary>
		/// Initializes a new instance of the class.
		/// </summary>
		public ENConnectionFailedException() : base()
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public ENConnectionFailedException(string message) : base(message)
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the
		/// current exception, or a null reference if no inner exception is
		/// specified</param>
		public ENConnectionFailedException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		// Constructor required for serialization
		[SecuritySafeCritical()]
		protected ENConnectionFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

	}

	[Serializable()]
	public class ENAuthExpiredException : System.Exception
	{
		/// <summary>
		/// Initializes a new instance of the class.
		/// </summary>
		public ENAuthExpiredException() : base()
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public ENAuthExpiredException(string message) : base(message)
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the
		/// current exception, or a null reference if no inner exception is
		/// specified</param>
		public ENAuthExpiredException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		// Constructor required for serialization
		[SecuritySafeCritical()]
		protected ENAuthExpiredException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

	}

	[Serializable()]
	public class ENInvalidDataException : System.Exception
	{
		/// <summary>
		/// Initializes a new instance of the class.
		/// </summary>
		public ENInvalidDataException() : base()
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public ENInvalidDataException(string message) : base(message)
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the
		/// current exception, or a null reference if no inner exception is
		/// specified</param>
		public ENInvalidDataException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		// Constructor required for serialization
		[SecuritySafeCritical()]
		protected ENInvalidDataException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

	}

	[Serializable()]
	public class ENNotFoundException : System.Exception
	{
		/// <summary>
		/// Initializes a new instance of the class.
		/// </summary>
		public ENNotFoundException() : base()
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public ENNotFoundException(string message) : base(message)
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the
		/// current exception, or a null reference if no inner exception is
		/// specified</param>
		public ENNotFoundException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		// Constructor required for serialization
		[SecuritySafeCritical()]
		protected ENNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

	}

	[Serializable()]
	public class ENPermissionDeniedException : System.Exception
	{
		/// <summary>
		/// Initializes a new instance of the class.
		/// </summary>
		public ENPermissionDeniedException() : base()
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public ENPermissionDeniedException(string message) : base(message)
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the
		/// current exception, or a null reference if no inner exception is
		/// specified</param>
		public ENPermissionDeniedException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		// Constructor required for serialization
		[SecuritySafeCritical()]
		protected ENPermissionDeniedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

	}

	[Serializable()]
	public class ENLimitReachedException : System.Exception
	{
		/// <summary>
		/// Initializes a new instance of the class.
		/// </summary>
		public ENLimitReachedException() : base()
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public ENLimitReachedException(string message) : base(message)
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the
		/// current exception, or a null reference if no inner exception is
		/// specified</param>
		public ENLimitReachedException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		// Constructor required for serialization
		[SecuritySafeCritical()]
		protected ENLimitReachedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

	}

	[Serializable()]
	public class ENQuotaReachedException : System.Exception
	{
		/// <summary>
		/// Initializes a new instance of the class.
		/// </summary>
		public ENQuotaReachedException() : base()
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public ENQuotaReachedException(string message) : base(message)
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the
		/// current exception, or a null reference if no inner exception is
		/// specified</param>
		public ENQuotaReachedException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		// Constructor required for serialization
		[SecuritySafeCritical()]
		protected ENQuotaReachedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

	}

	[Serializable()]
	public class ENDataConflictException : System.Exception
	{
		/// <summary>
		/// Initializes a new instance of the class.
		/// </summary>
		public ENDataConflictException() : base()
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public ENDataConflictException(string message) : base(message)
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the
		/// current exception, or a null reference if no inner exception is
		/// specified</param>
		public ENDataConflictException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		// Constructor required for serialization
		[SecuritySafeCritical()]
		protected ENDataConflictException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

	}

	[Serializable()]
	public class ENENMLInvalidException : System.Exception
	{
		/// <summary>
		/// Initializes a new instance of the class.
		/// </summary>
		public ENENMLInvalidException() : base()
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public ENENMLInvalidException(string message) : base(message)
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the
		/// current exception, or a null reference if no inner exception is
		/// specified</param>
		public ENENMLInvalidException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		// Constructor required for serialization
		[SecuritySafeCritical()]
		protected ENENMLInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

	}

	[Serializable()]
	public class ENRateLimitReachedException : System.Exception
	{
		/// <summary>
		/// Initializes a new instance of the class.
		/// </summary>
		public ENRateLimitReachedException() : base()
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public ENRateLimitReachedException(string message) : base(message)
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the
		/// current exception, or a null reference if no inner exception is
		/// specified</param>
		public ENRateLimitReachedException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		// Constructor required for serialization
		[SecuritySafeCritical()]
		protected ENRateLimitReachedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

	}

	[Serializable()]
	public class ENCancelledException : System.Exception
	{
		/// <summary>
		/// Initializes a new instance of the class.
		/// </summary>
		public ENCancelledException() : base()
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public ENCancelledException(string message) : base(message)
		{
		}
		/// <summary>
		/// Initializes a new instance of the class with the specified error message
		/// and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the
		/// current exception, or a null reference if no inner exception is
		/// specified</param>
		public ENCancelledException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		// Constructor required for serialization
		[SecuritySafeCritical()]
		protected ENCancelledException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

	}

}