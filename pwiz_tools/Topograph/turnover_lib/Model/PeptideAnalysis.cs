﻿/*
 * Original author: Nicholas Shulman <nicksh .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2009 University of Washington - Seattle, WA
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Criterion;
using pwiz.Topograph.Data;
using pwiz.Topograph.Enrichment;

namespace pwiz.Topograph.Model
{
    public class PeptideAnalysis : AnnotatedEntityModel<DbPeptideAnalysis>
    {
        private int _minCharge;
        private int _maxCharge;
        private int _intermediateLevels;
        private TurnoverCalculator _turnoverCalculator;
        private int _chromatogramRefCount;
        private WorkspaceVersion _workspaceVersion;

        public PeptideAnalysis(Workspace workspace, DbPeptideAnalysis dbPeptideAnalysis) : base(workspace, dbPeptideAnalysis)
        {
            FileAnalyses = new PeptideFileAnalyses(this, dbPeptideAnalysis);
            PeptideRates = new PeptideRates(this, dbPeptideAnalysis);
            SetWorkspaceVersion(workspace.WorkspaceVersion);
        }

        public Peptide Peptide { get; private set; }
        public PeptideRates PeptideRates { get; private set; }
        public PeptideFileAnalyses FileAnalyses { get; private set; }
        protected override void Load(DbPeptideAnalysis entity)
        {
            base.Load(entity);
            Peptide = Workspace.Peptides.GetPeptide(entity.Peptide);
            _minCharge = entity.MinCharge;
            _maxCharge = entity.MaxCharge;
            _intermediateLevels = entity.IntermediateEnrichmentLevels;
            ExcludedMzs = new ExcludedMzs(this);
            ExcludedMzs.ChangedEvent += ExcludedMzs_ChangedEvent;
            if (entity.ExcludedMasses != null)
            {
                ExcludedMzs.SetByteArray(entity.ExcludedMasses);
            }
            _workspaceVersion = Workspace.SavedWorkspaceVersion;
        }

        void ExcludedMzs_ChangedEvent(ExcludedMzs obj)
        {
            OnChange();
        }

        protected override DbPeptideAnalysis UpdateDbEntity(ISession session)
        {
            var entity = base.UpdateDbEntity(session);
            entity.MinCharge = MinCharge;
            entity.MaxCharge = MaxCharge;
            entity.IntermediateEnrichmentLevels = IntermediateLevels;
            entity.ExcludedMasses = ExcludedMzs.ToByteArray();
            if (PeptideRates.IsDirty)
            {
                entity.PeptideRateCount = 0;
            }
            return entity;
        }

        public void SaveDeep(ISession session)
        {
            Save(session);
            foreach (var fileAnalysis in FileAnalyses.ListChildren())
            {
                fileAnalysis.Save(session);
                fileAnalysis.Peaks.Save(session);
                fileAnalysis.PeptideDistributions.Save(session);
            }
            PeptideRates.Save(session);
        }

        public Dictionary<int,IList<MzRange>> GetMzs()
        {
            var result = new Dictionary<int, IList<MzRange>>();
            for (int charge = MinCharge; charge <= MaxCharge; charge ++)
            {
                result.Add(charge, GetTurnoverCalculator().GetMzs(charge));
            }
            return result;
        }

        public int MinCharge 
        { 
            get
            {
                return _minCharge;
            }
            set
            {
                using (GetWriteLock())
                {
                    if (_minCharge == value)
                    {
                        return;
                    }
                    _minCharge = value;
                    //InvalidateChromatograms();
                    OnChange();
                }
            }
        }

        public void InvalidateChromatograms()
        {
            foreach (var fileAnalysis in FileAnalyses.ListChildren())
            {
                fileAnalysis.InvalidateChromatograms();
            }
        }
        public int MaxCharge 
        { 
            get
            {
                return _maxCharge;
            }
            set
            {
                using (GetWriteLock())
                {
                    if (_maxCharge == value)
                    {
                        return;
                    }
                    _maxCharge = value;
                    //InvalidateChromatograms();
                    OnChange();
                }
            } 
        }

        public int GetMassCount()
        {
            return GetTurnoverCalculator().MassCount;
        }

        public int IntermediateLevels
        {
            get
            {
                return _intermediateLevels;
            }
            set
            {
                SetIfChanged(ref _intermediateLevels, value);
            }
        }
        public ExcludedMzs ExcludedMzs { get; private set; }

        public TurnoverCalculator GetTurnoverCalculator()
        {
            using(GetReadLock())
            {
                var turnoverCalculator = _turnoverCalculator;
                if (turnoverCalculator != null)
                {
                    return turnoverCalculator;
                }
                _turnoverCalculator = turnoverCalculator = new TurnoverCalculator(Workspace, Peptide.Sequence);
                return turnoverCalculator;
            }
        }

        public IList<PeptideFileAnalysis> GetFileAnalyses(bool filterRejects)
        {
            return FileAnalyses.ListPeptideFileAnalyses(filterRejects);
        }
        public PeptideFileAnalysis GetFileAnalysis(long id)
        {
            return FileAnalyses.GetChild(id);
        }
        public String GetLabel()
        {
            String label = Peptide.Sequence;
            if (label.Length > 15)
            {
                label = label.Substring(0, 5) + "..." + label.Substring(label.Length - 7, 7);
            }
            return label;
        }
        public int GetChromatogramRefCount()
        {
            return _chromatogramRefCount;
        }
        public void IncChromatogramRefCount()
        {
            _chromatogramRefCount++;
        }
        public void DecChromatogramRefCount()
        {
            _chromatogramRefCount--;
        }
        public void SetWorkspaceVersion(WorkspaceVersion newWorkspaceVersion)
        {
            if (!_workspaceVersion.PeptideRatesValid(newWorkspaceVersion))
            {
                PeptideRates = new PeptideRates(this);
            }
            _workspaceVersion = newWorkspaceVersion;
            _turnoverCalculator = null;
            foreach (var peptideFileAnalysis in FileAnalyses.ListChildren())
            {
                peptideFileAnalysis.SetWorkspaceVersion(newWorkspaceVersion);
            }
        }
        public void SetPeptideRates(PeptideRates peptideRates)
        {
            PeptideRates = peptideRates;
        }
        public void RecalculateRates()
        {
            var peptideRates = new PeptideRates(this);
            var peptideDistributionsList = new List<PeptideDistributions>();
            bool isComplete = true;
            foreach (var peptideFileAnalysis in FileAnalyses.ListPeptideFileAnalyses(true))
            {
                var peptideDistributions = peptideFileAnalysis.PeptideDistributions;
                if (peptideDistributions.ChildCount == 0)
                {
                    isComplete = false;
                    continue;
                }
                peptideDistributionsList.Add(peptideDistributions);
            }
            peptideRates.Calculate(peptideDistributionsList, isComplete);
            PeptideRates = peptideRates;
        }
    }
    public enum IntensityScaleMode
    {
        none,
        relative_include_all,
        relative_exclude_any_charge,
        relative_total,
    }
}
