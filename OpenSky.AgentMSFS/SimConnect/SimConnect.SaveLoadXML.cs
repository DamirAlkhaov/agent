﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimConnect.SaveLoadXML.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.SimConnect
{
    using System;
    using System.Device.Location;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Media;
    using System.Xml.Linq;

    using Microsoft.Maps.MapControl.WPF;

    using OpenSky.AgentMSFS.Models;
    using OpenSky.AgentMSFS.SimConnect.Structs;
    using OpenSky.AgentMSFS.Tools;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Simconnect client - flight save/load code.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class SimConnect
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Identifier for the agent.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private const string AgentIdentifier = "OpenSky.AgentMSFS";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The save file version.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private const string SaveFileVersion = "1.0";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Restores a save file.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/04/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="saveFile">
        /// The save file xml.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void RestoreSaveFile(string saveFile)
        {
            if (this.TrackingStatus != TrackingStatus.Resuming || this.Flight == null)
            {
                Debug.WriteLine("Not loading save file cause we are either not in resuming state or there is no active flight");
                return;
            }

            // Parse xml into memory
            var save = XElement.Parse(saveFile);

            // ========================================================================================================================
            // CHECK THE SAVE FILE FOR INCOMPATIBILITY OR TAMPERING
            // ========================================================================================================================

            // Do some basic checks
            if (!AgentIdentifier.Equals(save.Element("Agent")?.Value))
            {
                throw new Exception("This save file was generated by a different agent!");
            }

            if (!SaveFileVersion.Equals(save.Element("SaveFileVersion")?.Value))
            {
                throw new Exception("This save file is using a different save file version and cannot be loaded!");
            }

            // Check if flight basics still match
            var flightFromSave = save.EnsureChildElement("Flight");
            this.Flight.CheckSaveMatchesFlight(flightFromSave);

            // ========================================================================================================================
            // ALL CHECKS PASSED, RESTORE THE FLIGHT
            // ========================================================================================================================

            // Restore basic tracking stats
            this.trackingStarted = DateTime.ParseExact(save.EnsureChildElement("TrackingStarted").Value, "O", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            this.WasAirborne = bool.Parse(save.EnsureChildElement("WasAirborne").Value);
            this.timeSavedBecauseOfSimRate = TimeSpan.Parse(save.EnsureChildElement("WarpTimeSaved").Value);
            this.WarpInfo = this.timeSavedBecauseOfSimRate.TotalSeconds >= 1 ? $"Yes, saved {this.timeSavedBecauseOfSimRate:hh\\:mm\\:ss} [*]" : "No [*]";
            this.totalPaused = TimeSpan.Parse(save.EnsureChildElement("TotalPaused").Value);

            // Restore aircraft trail locations
            UpdateGUIDelegate restorePositionReports = () =>
            {
                var positionReports = save.EnsureChildElement("PositionReports");
                this.AircraftTrailLocations.Clear();
                foreach (var location in positionReports.Elements("Location"))
                {
                    this.AircraftTrailLocations.Add(new AircraftTrailLocation(location));
                }
            };
            Application.Current.Dispatcher.BeginInvoke(restorePositionReports);

            // Restore map event markers
            UpdateGUIDelegate restoreMapEventMarkers = () =>
            {
                var eventMapMarkers = save.EnsureChildElement("EventMapMarkers");
                lock (this.trackingEventMarkers)
                {
                    this.trackingEventMarkers.Clear();

                    // Add airport markers to map
                    var origin = flightFromSave.EnsureChildElement("Origin");
                    var originMarker = new TrackingEventMarker(new GeoCoordinate(double.Parse(origin.Attribute("Lat")?.Value ?? "0"), double.Parse(origin.Attribute("Lon")?.Value ?? "0")), origin.Attribute("ICAO")?.Value, OpenSkyColors.OpenSkyTeal, Colors.White);
                    this.trackingEventMarkers.Add(originMarker);
                    this.TrackingEventMarkerAdded?.Invoke(this, originMarker);

                    var alternate = flightFromSave.EnsureChildElement("Alternate");
                    var alternateMarker = new TrackingEventMarker(new GeoCoordinate(double.Parse(alternate.Attribute("Lat")?.Value ?? "0"), double.Parse(alternate.Attribute("Lon")?.Value ?? "0")), alternate.Attribute("ICAO")?.Value, OpenSkyColors.OpenSkyWarningOrange, Colors.Black);
                    this.trackingEventMarkers.Add(alternateMarker);
                    this.TrackingEventMarkerAdded?.Invoke(this, alternateMarker);

                    var destination = flightFromSave.EnsureChildElement("Destination");
                    var destinationMarker = new TrackingEventMarker(new GeoCoordinate(double.Parse(destination.Attribute("Lat")?.Value ?? "0"), double.Parse(destination.Attribute("Lon")?.Value ?? "0")), destination.Attribute("ICAO")?.Value, OpenSkyColors.OpenSkyTeal, Colors.White);
                    this.trackingEventMarkers.Add(destinationMarker);
                    this.TrackingEventMarkerAdded?.Invoke(this, destinationMarker);

                    foreach (var marker in eventMapMarkers.Elements("Marker"))
                    {
                        this.trackingEventMarkers.Add(new TrackingEventMarker(marker));
                    }
                }
            };
            Application.Current.Dispatcher.BeginInvoke(restoreMapEventMarkers);

            // Restore event log entries
            UpdateGUIDelegate restoreEventLog = () =>
            {
                var eventLog = save.EnsureChildElement("EventLog");
                this.TrackingEventLogEntries.Clear();
                foreach (var logEntry in eventLog.Elements("LogEntry"))
                {
                    this.TrackingEventLogEntries.Add(new TrackingEventLogEntry(logEntry));
                }
            };
            Application.Current.Dispatcher.BeginInvoke(restoreEventLog);

            // Restore landing reports
            var landingReport = save.EnsureChildElement("LandingReport");
            this.LandingReports.Clear();
            foreach (var touchdown in landingReport.Elements("Touchdown"))
            {
                this.LandingReports.Add(new LandingReport(touchdown));
            }

            // Restore simbrief waypoint markers
            UpdateGUIDelegate restoreSimbrief = () =>
            {
                var simbriefWaypoints = save.EnsureChildElement("SimbriefWaypoints");
                this.SimbriefRouteLocations.Clear();
                this.simbriefWaypointMarkers.Clear();
                this.SimbriefRouteLocations.Add(new Location(this.Flight.Origin.Latitude, this.Flight.Origin.Longitude));
                foreach (var waypoint in simbriefWaypoints.Elements("Waypoint"))
                {
                    var waypointMarker = new SimbriefWaypointMarker(waypoint);
                    this.simbriefWaypointMarkers.Add(waypointMarker);
                    this.SimbriefRouteLocations.Add(new Location(waypointMarker.GeoCoordinate.Latitude, waypointMarker.GeoCoordinate.Longitude));
                }

                this.SimbriefRouteLocations.Add(new Location(this.Flight.Destination.Latitude, this.Flight.Destination.Longitude));
            };
            Application.Current.Dispatcher.BeginInvoke(restoreSimbrief);

            // Restore fuel tanks, payload stations and slew plane into position structs into temporary holders until the user clicks resume
            this.flightLoadingTempStructs = new FlightLoadingTempStructs
            {
                FuelTanks = FuelTanksSaver.RestoreFuelTanksFromSave(save.EnsureChildElement("FuelTanks")),
                PayloadStations = PayloadStationsSaver.RestorePayloadStationsFromSave(save.EnsureChildElement("PayloadStations")),
                SlewPlaneIntoPosition = SlewPlaneIntoPositionSaver.RestoreSlewPlaneIntoPositionFromSave(save.EnsureChildElement("ResumePosition"))
            };

            // Now that everything has been loaded, replay any and all map markers
            this.ReplayMapMarkers();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flight loading temporary structs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private FlightLoadingTempStructs flightLoadingTempStructs;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates a XML save file for the current flight.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/03/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private XElement GenerateSaveFile()
        {
            if ((this.TrackingStatus != TrackingStatus.Tracking && this.TrackingStatus != TrackingStatus.GroundOperations) || this.Flight == null)
            {
                Debug.WriteLine("Not generating save file cause we are either not tracking or there is no active flight");
                return null;
            }

            try
            {
                Debug.WriteLine("Generating flight save XML file...");
                var save = new XElement("OpenSky.SavedFlight");
                save.Add(new XElement("SaveFileVersion", SaveFileVersion));

                // Add some basic info about this save
                save.Add(new XElement("Agent", AgentIdentifier));
                save.Add(new XElement("AgentVersion", Assembly.GetExecutingAssembly().GetName().Version.ToString()));
                save.Add(new XElement("OpenSkyUser", UserSessionService.Instance.Username));
                save.Add(new XElement("LocalTimeZone", $"{TimeZoneInfo.Local.BaseUtcOffset.TotalHours}"));
                save.Add(new XElement("TrackingStarted", $"{this.trackingStarted:O}"));
                save.Add(new XElement("TrackingStopped", $"{DateTime.UtcNow:O}"));
                save.Add(new XElement("WasAirborne", this.WasAirborne));
                save.Add(new XElement("WarpTimeSaved", $"{this.timeSavedBecauseOfSimRate:c}"));
                save.Add(new XElement("TotalPaused", $"{this.totalPaused:c}"));

                // Add basic flight data
                save.Add(this.Flight.GenerateFlightForSave());

                // Add flight events
                var eventLog = new XElement("EventLog");
                save.Add(eventLog);
                // ReSharper disable once ForCanBeConvertedToForeach - we need to iterate this the old fashioned way because these collections could still get some events added
                for (var i = 0; i < this.TrackingEventLogEntries.Count; i++)
                {
                    eventLog.Add(this.TrackingEventLogEntries[i].GetLogEntryForSave());
                }

                // Add tracking map markers
                var trackingMapMarkers = new XElement("EventMapMarkers");
                save.Add(trackingMapMarkers);
                lock (this.trackingEventMarkers)
                {
                    // ReSharper disable once ForCanBeConvertedToForeach - we need to iterate this the old fashioned way because these collections could still get some events added
                    for (var i = 0; i < this.trackingEventMarkers.Count; i++)
                    {
                        var marker = this.trackingEventMarkers[i].GetEventMarkerForSave();

                        // Marker can decide it doesn't need to be saved and return null
                        if (marker != null)
                        {
                            trackingMapMarkers.Add(marker);
                        }
                    }
                }

                // Add flight position reports
                var aircraftTrail = new XElement("PositionReports");
                save.Add(aircraftTrail);
                // ReSharper disable once ForCanBeConvertedToForeach - we need to iterate this the old fashioned way because these collections could still get some events added
                for (var i = 0; i < this.AircraftTrailLocations.Count; i++)
                {
                    if (this.AircraftTrailLocations[i] is AircraftTrailLocation trail)
                    {
                        aircraftTrail.Add(trail.GetLocationForSave());
                    }
                }

                // Add landing reports
                var landingReport = new XElement("LandingReport");
                save.Add(landingReport);
                // ReSharper disable once ForCanBeConvertedToForeach - we need to iterate this the old fashioned way because these collections could still get some events added
                for (var i = 0; i < this.LandingReports.Count; i++)
                {
                    landingReport.Add(this.LandingReports[i].GetLandingReportForSave());
                }

                // Add simbrief flight plan waypoints
                var simbriefWaypoints = new XElement("SimbriefWaypoints");
                save.Add(simbriefWaypoints);
                // ReSharper disable once ForCanBeConvertedToForeach - we need to iterate this the old fashioned way because these collections could still get some events added
                for (var i = 0; i < this.simbriefWaypointMarkers.Count; i++)
                {
                    simbriefWaypoints.Add(this.simbriefWaypointMarkers[i].GetWaypointMarkerForSave());
                }

                // Save fuel tanks
                save.Add(FuelTanksSaver.GetFuelTanksForSave(this.FuelTanks));

                // Save payload stations
                save.Add(PayloadStationsSaver.GetPayloadStationsForSave(this.PayloadStations));

                // Save aircraft last position
                save.Add(SlewPlaneIntoPositionSaver.GetSlewPlaneIntoPositionForSave(this.PrimaryTracking));

                Debug.WriteLine("Flight save file created successfully");
                return save;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Flight save file creation failed: " + ex);
                throw;
            }
        }
    }
}