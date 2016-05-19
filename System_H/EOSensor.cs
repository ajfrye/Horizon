﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HSFUniverse;
using MissionElements;
using System.Xml;
using HSFSystem;
using Utilities;

namespace HSFSubsystem
{
    public class EOSensor : Subsystem
    {
        #region Attributes
        //Default stuff
        public static string SUBNAME_EOSENSOR = "EOSensor";
        private StateVarKey<double> PIXELS_KEY;
        private StateVarKey<double> INCIDENCE_KEY;
        private StateVarKey<bool> EOON_KEY;
        private double _lowQualityPixels = 5000;
        private double _lowQualityTime = 3;
        private double _midQualityPixels = 10000;
        private double _midQualityTime = 5;
        private double _highQualityPixels = 15000;
        private double _highQualityTime = 7;
        #endregion

        #region Constructors
        public EOSensor(XmlNode EOSensorXmlNode, Dependencies dependencies, Asset asset)
        {
            DefaultSubName = "EOSensor";
            Asset = asset;
            getSubNameFromXmlNode(EOSensorXmlNode);
            PIXELS_KEY = new StateVarKey<double>(Asset.Name +"." + "numpixels");
            INCIDENCE_KEY = new StateVarKey<double>(Asset.Name + "." + "incidenceangle");
            EOON_KEY = new StateVarKey<bool>(Asset.Name + "." + "eosensoron");
            addKey(PIXELS_KEY);
            addKey(INCIDENCE_KEY);
            addKey(EOON_KEY);
            DependentSubsystems = new List<Subsystem>();
            SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
            if (EOSensorXmlNode.Attributes["lowQualityPixels"] != null)
                _lowQualityPixels = (double)Convert.ChangeType(EOSensorXmlNode.Attributes["lowQualityPixels"].Value.ToString(), typeof(double));
            if (EOSensorXmlNode.Attributes["lowQualityPixels"] != null)
                _lowQualityTime = (double)Convert.ChangeType(EOSensorXmlNode.Attributes["lowQualityTime"].Value.ToString(), typeof(double));
            if (EOSensorXmlNode.Attributes["midQualityPixels"] != null)
                _midQualityPixels = (double)Convert.ChangeType(EOSensorXmlNode.Attributes["midQualityPixels"].Value.ToString(), typeof(double));
            if (EOSensorXmlNode.Attributes["midQualityTime"] != null)
                _midQualityTime = (double)Convert.ChangeType(EOSensorXmlNode.Attributes["midQualityTime"].Value.ToString(), typeof(double));
            if (EOSensorXmlNode.Attributes["highQualityPixels"] != null)
                _highQualityPixels = (double)Convert.ChangeType(EOSensorXmlNode.Attributes["highQualityPixels"].Value.ToString(), typeof(double));
            if (EOSensorXmlNode.Attributes["highQualityTime"] != null)
                _highQualityTime = (double)Convert.ChangeType(EOSensorXmlNode.Attributes["highQualityTime"].Value.ToString(), typeof(double));
            dependencies.Add("PowerfromEOSensor", new Func<Event, HSFProfile<double>>(POWERSUB_PowerProfile_EOSENSORSUB));
        }
        #endregion

        #region Methods
        public override bool canPerform(Event proposedEvent, Universe environment)
        {
            if (!base.canPerform(proposedEvent, environment))
                return false;
            if (_task.Type == TaskType.IMAGING)
            {
                //set pixels and time to caputre based on target value
                int value = _task.Target.Value;
                double pixels = _lowQualityPixels;
                double timetocapture = _lowQualityTime;
                if (value <= _highQualityTime && value >= _midQualityTime) //Morgan took out magic numbers
                {
                    pixels = _midQualityPixels;
                    timetocapture = _midQualityTime;
                }
                if (value > _highQualityTime)
                {
                    pixels = _highQualityPixels;
                    timetocapture = _highQualityTime;
                }

                // get event start and task start times
                double es = proposedEvent.GetEventStart(Asset);
                double ts = proposedEvent.GetTaskStart(Asset);
                double te = proposedEvent.GetTaskEnd(Asset);
                if (ts > te)
                {
                    Logger.Report("EOSensor lost access");
                    return false;
                }

                // set task end based upon time to capture
                te = ts + timetocapture;
                proposedEvent.SetTaskEnd(Asset, te);

                // calculate incidence angle
                // from Brown, Pp. 99
                DynamicState position = Asset.AssetDynamicState;
                double timage = ts + timetocapture / 2;
                Matrix<double> m_SC_pos_at_tf_ECI = position.PositionECI(timage);
                Matrix<double> m_target_pos_at_tf_ECI = _task.Target.DynamicState.PositionECI(timage);
                Matrix<double> m_pv = m_target_pos_at_tf_ECI - m_SC_pos_at_tf_ECI;
                Matrix<double> pos_norm = -m_SC_pos_at_tf_ECI / Matrix<double>.Norm(-m_SC_pos_at_tf_ECI);
                Matrix<double> pv_norm = m_pv / Matrix<double>.Norm(m_pv);

                double incidenceang = 90 - 180 / Math.PI * Math.Acos(Matrix<double>.Dot(pos_norm, pv_norm));

                // set state data
                _newState.addValue(INCIDENCE_KEY, new KeyValuePair<double, double>(timage, incidenceang));
                _newState.addValue(INCIDENCE_KEY, new KeyValuePair<double, double>(timage + 1, 0.0));

                _newState.addValue(PIXELS_KEY, new KeyValuePair<double, double>(timage, pixels));
                _newState.addValue(PIXELS_KEY, new KeyValuePair<double, double>(timage + 1, 0.0));

                _newState.addValue(EOON_KEY, new KeyValuePair<double, bool>(ts, true));
                _newState.addValue(EOON_KEY, new KeyValuePair<double, bool>(te, false));
            }
                return true;
            
        }

        HSFProfile<double> POWERSUB_PowerProfile_EOSENSORSUB(Event currentEvent)
        {
            HSFProfile<double> prof1 = new HSFProfile<double>();
            prof1[currentEvent.GetEventStart(Asset)] = 10;
            if (currentEvent.State.getValueAtTime(EOON_KEY, currentEvent.GetTaskStart(Asset)).Value)
            {
                prof1[currentEvent.GetTaskStart(Asset)] = 60;
                prof1[currentEvent.GetTaskEnd(Asset)] = 10;
            }
            return prof1;
        }
        /// <summary>
        /// Dependecy function for the SSDR subsystem
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        HSFProfile<double> SSDRSUB_NewDataProfile_EOSENSORSUB(SystemState state)
        {
            return state.GetProfile(PIXELS_KEY) / 500;
        }
        #endregion
    }
}
