﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Toolchains;

namespace BenchmarkDotNet.Characteristics
{
    public abstract class CharacteristicSetPresenter
    {
        public static readonly CharacteristicSetPresenter Default = new DefaultPresenter();
        public static readonly CharacteristicSetPresenter Display = new DisplayPresenter();
        public static readonly CharacteristicSetPresenter Folder = new FolderPresenter();
        public static readonly CharacteristicSetPresenter SourceCode = new SourceCodePresenter();

        public abstract string ToPresentation(CharacteristicObject obj);

        protected virtual IEnumerable<Characteristic> GetPresentableCharacteristics(CharacteristicObject obj, bool includeIgnoreOnApply = false) =>
            obj
                .GetCharacteristicsWithValues()
                .Where(c => c.IsPresentableCharacteristic(includeIgnoreOnApply));

        private class DefaultPresenter : CharacteristicSetPresenter
        {
            private const string Separator = "&";
            private static readonly CharacteristicPresenter CharacteristicPresenter = CharacteristicPresenter.DefaultPresenter;

            public override string ToPresentation(CharacteristicObject obj)
            {
                var values = GetPresentableCharacteristics(obj)
                    .Select(c => c.FullId + "=" + CharacteristicPresenter.ToPresentation(obj, c));
                return string.Join(Separator, values);
            }
        }

        private class FolderPresenter : CharacteristicSetPresenter
        {
            private const string Separator = "_";
            private const string EqualsSeparator = "-";
            private static readonly CharacteristicPresenter CharacteristicPresenter = CharacteristicPresenter.FolderPresenter;

            public override string ToPresentation(CharacteristicObject obj)
            {
                var values = GetPresentableCharacteristics(obj)
                    .Select(c => c.Id + EqualsSeparator + CharacteristicPresenter.ToPresentation(obj, c));
                return string.Join(Separator, values);
            }
        }

        private class DisplayPresenter : CharacteristicSetPresenter
        {
            private const string Separator = ", ";
            private static readonly CharacteristicPresenter CharacteristicPresenter = CharacteristicPresenter.DefaultPresenter;

            public override string ToPresentation(CharacteristicObject obj)
            {
                var values = GetPresentableCharacteristics(obj)
                    .Select(c => c.Id + "=" + CharacteristicPresenter.ToPresentation(obj, c));
                return string.Join(Separator, values);
            }
        }

        private class SourceCodePresenter : CharacteristicSetPresenter
        {
            private const string Separator = "; ";
            private static readonly CharacteristicPresenter CharacteristicPresenter = CharacteristicPresenter.SourceCodePresenter;
            private static readonly HashSet<Type> NonExportableTypes = new HashSet<Type>
            {
                typeof(IToolchain) // there is no need to set toolchain in child process, it was causing parameterless ctor requirement for all IToolchain implementations
            };

            public override string ToPresentation(CharacteristicObject obj)
                => string.Join(Separator, 
                        GetPresentableCharacteristics(obj, includeIgnoreOnApply: true)
                            .Select(c => CharacteristicPresenter.ToPresentation(obj, c)));

            protected override IEnumerable<Characteristic> GetPresentableCharacteristics(CharacteristicObject obj, bool includeIgnoreOnApply = false)
                => base.GetPresentableCharacteristics(obj, includeIgnoreOnApply)
                       .Where(characteristic => !NonExportableTypes.Contains(characteristic.CharacteristicType));
        }
    }
}