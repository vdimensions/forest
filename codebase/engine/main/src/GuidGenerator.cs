using System;

namespace Forest
{
    internal static class GuidGenerator
    {
        /// <summary>
        /// Generate a new <see cref="Guid"/> using the comb algorithm.
        /// <para>
        /// Implementation references from: 
        /// <para>https://stackoverflow.com/a/12580020/795158</para>
        /// <para>https://github.com/nhibernate/nhibernate-core/blob/5e71e83ac45439239b9028e6e87d1a8466aba551/src/NHibernate/Id/GuidCombGenerator.cs</para>
        /// </para>
        /// </summary>
        public static Guid NewID()
        {
            var guidArray = Guid.NewGuid().ToByteArray();

            var baseDate = new DateTime(1900, 1, 1);
            var now = DateTime.UtcNow;

            // Get the days and milliseconds which will be used to build the byte string 
            var days = new TimeSpan(now.Ticks - baseDate.Ticks);
            var milliseconds = now.TimeOfDay.TotalMilliseconds;

            // Convert to a byte array 
            // Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333 
            var daysArray = BitConverter.GetBytes(days.Days);
            var msecsArray = BitConverter.GetBytes((long)(milliseconds / 3.333333));

            // Reverse the bytes to match SQL Servers ordering 
            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);

            // Copy the bytes into the guid 
            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

            return new Guid(guidArray);
        }
    }
}