using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using ExpressionDebugger;
using MappingTest.Dtos;
using MappingTest.Models;
using Mapster;
using Shouldly;
using Shouldly.Configuration;
using Xunit;

namespace MappingTest
{
    public class HomeMappingTest
    {
        private Home _home;
        private Address _address;
        private Owner _owner;
        private static readonly Guid _ownerId = Guid.NewGuid();

        public HomeMappingTest()
        {
            _home = InitHome();
            _address = InitAddress();
            _owner = InitOwner();
        }

        #region build Test

        private Home InitHome()
        {
            return new Home {Id = 2, Address = InitAddress(), Name = "zhangsan's home", Owner = InitOwner()};
        }

        private Address InitAddress()
        {
            return new Address {Id = 1, Name = "Beijing"};
        }

        private Owner InitOwner()
        {
            return new Owner {Id = _ownerId, FirstName = "zhang", LastName = "san"};
        }

        #endregion

        #region basic Test

        [Fact]
        public void should_mapping_address_dto()
        {
            var address = _address.Adapt<AddressDto>();
            address.ShouldNotBeNull();
            address.Name.ShouldBe("Beijing");
        }

        [Fact]
        public void should_mapping_address_dto_without_no_new_object()
        {
            var addressDto = new AddressDto("Beijing");
            _address.Adapt(addressDto);
            addressDto.ShouldNotBeNull();
            addressDto.Name.ShouldBe("Beijing");
        }

        #endregion

        #region Mapping with logic

        [Fact]
        public void mapping_ignore()
        {
            _home.Ignore = "ignore";
            var homeDto = _home.Adapt<HomeDto>();
            homeDto.Ignore.ShouldBeNull();
        }

        [Fact]
        public void should_mapping_owner_dto_for_with_mapping_logic()
        {
            var config_for_dto = TypeAdapterConfig<Owner, OwnerDto>.NewConfig()
                .Map(dest => dest.FullName, src => src.FirstName + src.LastName).Config;
            var ownerDto = _owner.Adapt<OwnerDto>(config_for_dto);
            ownerDto.ShouldNotBeNull();
            ownerDto.FullName.ShouldBe("zhangsan");
            ownerDto.Id.ShouldBe(_ownerId.ToString());

            var config_for_entity = TypeAdapterConfig<OwnerDto, Owner>.NewConfig()
                .Map(dest => dest.FirstName, src => src.FullName)
                .Map(dest => dest.LastName, src => src.FullName).Config;
            var owner = ownerDto.Adapt<Owner>(config_for_entity);
            owner.Id.ShouldBe(_ownerId);
            owner.FirstName.ShouldBe("zhangsan");
            owner.LastName.ShouldBe("zhangsan");
        }

        [Fact]
        public void should_mapping_owner_with_mapping_logic_for_common_type()
        {
            var config = TypeAdapterConfig<OwnerDto, Owner>.NewConfig()
                .Map(dest => dest.FirstName, src => src.FullName)
                .Map(dest => dest.LastName, src => src.FullName)
                .AddDestinationTransform((string x) => x.ToUpper())
                .Config;
            var ownerDto = new OwnerDto {Id = _ownerId.ToString(), FullName = "zhangsan"};
            var owner = ownerDto.Adapt<Owner>(config);
            owner.FirstName.ShouldBe("ZHANGSAN", Case.Sensitive);
            owner.LastName.ShouldBe("ZHANGSAN", Case.Sensitive);
        }

        #endregion


        [Fact]
        public void should_mapping_address_dto_with_specific_constructor()
        {
            var typeAdapterConfig = TypeAdapterConfig<Address, AddressDto>.NewConfig()
                //.MapToConstructor(true)
                // .Map(desc => desc.Name, src => src.Id)
                .ConstructUsing(desc => new AddressDto(desc.Id.ToString()))
                //.MapWith(source => new AddressDto(source.Id.ToString()))
                .Config;

            var addressDto = _address.Adapt<AddressDto>(typeAdapterConfig);
            addressDto.Name.ShouldBe(1.ToString());
        }

        [Fact]
        public void should_mapping_home_dto_list_in_linq()
        {
            var typeAdapterConfig = TypeAdapterConfig<Address, AddressDto>.NewConfig()
                .MapWith(source => new AddressDto(source.Name))
                .Config;
            var addresses = new List<Address>();
            for (int i = 0; i < 10; i++)
            {
                addresses.Add(new Address {Id = i, Name = $"home{i}"});
            }

            var addressDtos = addresses.Where(h => h.Id < 5).AsQueryable().ProjectToType<AddressDto>(typeAdapterConfig)
                .ToList();
            addressDtos.Count.ShouldBe(5);
            for (int i = 0; i < 5; i++)
            {
                addressDtos[i].Name.ShouldBe($"home{i}");
            }
        }

        [Fact]
        public void should_mapping_home_dto_correctly_when_using_config()
        {
            TypeAdapterConfig<Address, AddressDto>.NewConfig()
                .MapWith(src => new AddressDto(src.Name));
            TypeAdapterConfig<Home, HomeDto>.ForType()
                .Map(desc => desc.AddressName, source => source.Address.Name)
                .Map(desc => desc.OwnerName, source => $"{source.Owner.FirstName}{source.Owner.LastName}");

            var homeDto = _home.Adapt<HomeDto>();
            homeDto.ShouldNotBeNull();
            homeDto.Id.ShouldBe(2);
            homeDto.Name.ShouldBe("zhangsan's home");
            homeDto.Address.Name.ShouldBe("Beijing");
            homeDto.AddressName.ShouldBe("Beijing");
            homeDto.OwnerName.ShouldBe("zhangsan");
        }

        [Fact]
        public void using_builder_for_debug()
        {
            var opt = new ExpressionCompilationOptions {IsRelease = !Debugger.IsAttached};
            TypeAdapterConfig.GlobalSettings.Compiler = exp => exp.CompileWithDebugInfo(opt);
            var script = _owner
                .BuildAdapter()
                .CreateMapExpression<OwnerDto>()
                .ToScript();
            script.ShouldBe(
                "\npublic OwnerDto Main(Owner p1)" +
                "\n{" +
                "\n    return p1 == null ? null : new OwnerDto() {Id = p1.Id.ToString()};" +
                "\n}");
        }
    }
}