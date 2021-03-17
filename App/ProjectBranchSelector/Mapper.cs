using AutoMapper;
using ProjectBranchSelector.Common;
using ProjectBranchSelector.Models;
using ProjectBranchSelector.Db.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectBranchSelector
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            CreateMap<TreeCreator, Tree>()               
                .ForMember(s => s.Id, s => s.MapFrom(c=>Helper.GenerateGuid(new string[] { c.Name })))
                .ForMember(s=>s.VersionDate, s=>s.MapFrom(c=>DateTimeOffset.Now));

            CreateMap<TreeUpdater, Tree>()                
                .ForMember(s => s.Id, s => s.MapFrom(c => Helper.GenerateGuid(new string[] { c.Name })))               
                .ForMember(s => s.VersionDate, s => s.MapFrom(c => DateTimeOffset.Now));

            CreateMap<Tree, TreeModel>();

            CreateMap<TreeItem, TreeItemModel>();

            CreateMap<FormulaCreator, Formula>();

            CreateMap<FormulaUpdater, Formula>();            

            CreateMap<Formula, FormulaModel>();
            CreateMap<TreeHistory, TreeHistoryModel>();
            CreateMap<TreeItemHistory, TreeItemHistoryModel>();
            CreateMap<FormulaHistory, FormulaHistoryModel>();
        }
    }
}
