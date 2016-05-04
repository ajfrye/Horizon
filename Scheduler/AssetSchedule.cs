﻿using System.Collections.Generic;
using Utilities;
using MissionElements;
using HSFSystem;

namespace HSFScheduler
{
    public class AssetSchedule
    {
        public SystemState InitialState { get; private set; }
        public Stack<Event> Events { get; private set; }
        public Asset Asset { get; private set; }

        /**
         * Creates a new empty schedule with the given initial state.
         * @param initialState the initial state of the system for which the schedule applies
         */
        public AssetSchedule(SystemState state)
        {
            InitialState = state;
        }

        /**
	     * Creates a new endstate-safe schedule from the given schedule. (last state copied as deep copy, all others shallow copies)
	     * @param schedToMakeSafeCopyFrom the schedule to copy
	     */
        public AssetSchedule(AssetSchedule oldSchedule)
        {
            AssetSchedule newAssetSched = DeepCopy.Copy<AssetSchedule>(oldSchedule);
            InitialState = newAssetSched.InitialState;
            Events = newAssetSched.Events;
        }

        /**
	     * Creates a new assetSchedule from and old assetSchedule and a new Event shared pointer
	     * @param oldSchedule the old schedule to base this schedule off of
	     * @param newEvent the new event to add to the schedule
	     */
        public AssetSchedule(AssetSchedule oldSchedule, Event newEvent, Asset newAsset)
        {
            AssetSchedule newAssetSched = DeepCopy.Copy<AssetSchedule>(oldSchedule);
            InitialState = newAssetSched.InitialState;
            Events = newAssetSched.Events;
            Events.Push(newEvent);
            Asset = newAssetSched.Asset;

        }


        /**
        * Returns the last State in the schedule
        * @return the last State in the schedule
        */
        public SystemState GetLastState()
        {
            if (!isEmpty()) //TODO: check this is what we actually want to do
            {
                return Events.Peek().State;
            }
            else
                return InitialState;
        }

        /**
        * Returns the last Task in the schedule
        * @return the last Task in the schedule
        */
        public Task GetLastTask()
        {
            if (isEmpty() == false) //TODO: check that this is actually what we want to do.
            {
                return Events.Peek().Task;
            }
            else return null;
        }

        /**
        * Returns the number of times the specified task has been completed in this schedule
        * @param newTask the task to count the times completed
        * @return the number of times the task has been completed
        */
        public int timesCompletedTask(Task newTask)
        {
            int count = 0;
            foreach(Event eit in Events)
            {
                if (eit != null && newTask != null)
                {
                    if (eit.Task == newTask)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /**
        * Returns the number of events in the schedule.
        * @return the number of events in the schedule
        */
        public int size()
        {
            return Events.Count;
        }

        /**
        * Returns whether the schedule is empty
        * @return true if the schedule contains no events, false otherwise
        */
        public bool isEmpty()
        {
            if (Events.Count == 0)
                return true;
            return false;
        }
    }
}