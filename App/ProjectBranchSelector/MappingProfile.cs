//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//ref 1

using AutoMapper;
using ProjectBranchSelector.Common;
using ProjectBranchSelector.Db.Model;
using ProjectBranchSelector.Models;
using System;

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
