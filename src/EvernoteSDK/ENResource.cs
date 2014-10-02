using System;
using System.Drawing;
using System.Drawing.Imaging;
using Evernote.EDAM.Type;
using EvernoteSDK.Advanced;

namespace EvernoteSDK
{
	public class ENResource
	{
		private byte[] _data;
		public byte[] Data
		{
			get
			{
				return _data;
			}
			set
			{
				if (value != null && value.Length >= Int32.MaxValue)
				{
					ENSDKLogger.ENSDKLogError("Data length for resource is greater than Int32.");
					value = null;
				}

				DataHash = null;
				_data = value;
			}
		}

		public string MimeType {get; set;}
		public string Filename {get; set;}
		internal string SourceUrl {get; set;}
		private string Guid {get; set;}

		private byte[] _dataHash;
		internal byte[] DataHash
		{
			get
			{
				// Compute and cache the hash value.
				if (_dataHash == null && Data.Length > 0)
				{
					_dataHash = Data.Enmd5();
				}
				return _dataHash;
			}
			set
			{
				_dataHash = value;
			}
		}

		internal ENResource()
		{
		}

		public ENResource(byte[] data, string mimeType, string filename)
		{
			Data = data;
			MimeType = mimeType;
			Filename = filename;

			if (data == null)
			{
				throw new ArgumentException("Invalid argument", "data");
			}
		}

		public ENResource(byte[] data, string mimeType) : this(data, mimeType, null)
		{
		}

		public ENResource(Image image) : this()
		{
			System.IO.MemoryStream memstream = new System.IO.MemoryStream();
			image.Save(memstream, image.RawFormat);
			Data = memstream.ToArray();

			if (image.RawFormat.Equals(ImageFormat.Png))
			{
				MimeType = Evernote.EDAM.Limits.Constants.EDAM_MIME_TYPE_PNG;
			}
			else if (image.RawFormat.Equals(ImageFormat.Jpeg))
			{
				MimeType = Evernote.EDAM.Limits.Constants.EDAM_MIME_TYPE_JPEG;
				//ElseIf image.RawFormat.Equals(ImageFormat.Gif) Then
				// TODO: Find out if it's OK to include the GIF format
				//    Me.mimeType = Evernote.EDAM.Limits.Constants.EDAM_MIME_TYPE_GIF
			}
			else
			{
				throw new ArgumentException("Unsupported image format", "image");
			}
		}

		internal static ENResource ResourceWithServiceResource(Resource serviceResource)
		{
			if (serviceResource.Data.Body == null)
			{
				ENSDKLogger.ENSDKLogError("Can't create an ENResource from an EDAMResource with no body");
				return null;
			}

			var resource = new ENResource();
			resource.Data = serviceResource.Data.Body;
			resource.MimeType = serviceResource.Mime;
			resource.Filename = serviceResource.Attributes.FileName;
			resource.SourceUrl = serviceResource.Attributes.SourceURL;
			return resource;
		}

		internal Resource EDAMResource()
		{
			if (Data == null)
			{
				return null;
			}

			Resource resource = new Resource();
			resource.Guid = Guid;
			if (resource.Guid == null && Data != null)
			{
				resource.Data = new Data();
				resource.Data.BodyHash = DataHash;
				resource.Data.Size = Data.Length;
				resource.Data.Body = Data;
			}
			resource.Mime = MimeType;
			ResourceAttributes attributes = new ResourceAttributes();
			if (Filename != null)
			{
				attributes.FileName = Filename;
			}
			if (SourceUrl != null)
			{
				attributes.SourceURL = SourceUrl;
			}
			resource.Attributes = attributes;
			return resource;
		}

	}

}