using System;

namespace EvernoteSDK
{
	public static class TimeConversions_EvernoteSDK
	{
		public static DateTime ToDateTime(this long edamTimestamp)
		{
			try
			{
				TimeSpan ts = new TimeSpan((edamTimestamp * 10000));
				// Create a date with the standard web base of 01/01/1970, then
				// add the timespan difference.
				DateTime newDate = (new DateTime(1970, 1, 1)).Add(ts);
				// Adjust for the current timezone.
				ts = TimeZone.CurrentTimeZone.GetUtcOffset(newDate);
				newDate = newDate.Add(ts);
				return newDate;
			}
			catch (Exception)
			{
				return Convert.ToDateTime("12:00:00 AM");
			}
		}

		public static long ToEdamTimestamp(this DateTime theDate)
		{
			// Adjust for the current timezone.
			theDate = theDate.ToUniversalTime();
			// Get the ticks as a base for the standard web base date.
			long baseOffset = (new DateTime(1970, 1, 1)).Ticks;
			// Get the difference between the base and our date.
			long newDate = theDate.Ticks - baseOffset;
			// Convert from ticks to seconds.
			newDate = newDate / 10000;
			return newDate;
		}

	}

}