﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace StockManagerDB
{
    /// <summary>
    /// Define a project
    /// </summary>
    public class Project : ICloneable
    {
        public Project()
        {
        }

        public Project(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Name of the project. Unique amongst the projects
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// List of version identified by their unique VersionStr
        /// </summary>
        public SortedDictionary<string, ProjectVersion> Versions { get; set; } = new SortedDictionary<string, ProjectVersion>(new CompareVersion());

        public object Clone()
        {
            Project newProject = new Project
            {
                Name = Name
            };

            foreach (ProjectVersion version in Versions.Values)
            {
                newProject.Versions.Add(version.Version, version.Clone() as ProjectVersion);
            }

            return newProject;
        }

        public class CompareName : IComparer<Project>
        {
            public int Compare(Project x, Project y)
            {
                return x.Name.CompareTo(y.Name);
            }
        }

        public class CompareVersion : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                Version vx = Version.Parse(x);
                Version vy = Version.Parse(y);
                return vx.CompareTo(vy);
            }
        }
    }
}
