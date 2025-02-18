﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LandingAnalysis.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator.Models
{
    using Microsoft.Maps.MapControl.WPF;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Landing analysis model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class LandingAnalysis
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The true airspeed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double AirspeedTrue { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The altitude in feet.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Altitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The bank angle in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double BankAngle { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The g-force.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Gforce { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The ground speed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double GroundSpeed { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The landing rate in feet per minute.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double LandingRate => this.LandingRateSeconds * 60;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The vertical speed during touchdown (feet/s).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double LandingRateSeconds { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The latitude in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Latitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The current map location.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Location Location => new(this.Latitude, this.Longitude, this.Altitude);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The longitude in degrees.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double Longitude { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Is the plane on the ground?
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool OnGround { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The sidewards speed of the plane (feet/s).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double SpeedLat { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The forward speed of the plane (feet/s).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double SpeedLong { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The cross wind (knots).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double WindLat { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The headwind/tailwind (knots).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double WindLong { get; set; }
    }
}