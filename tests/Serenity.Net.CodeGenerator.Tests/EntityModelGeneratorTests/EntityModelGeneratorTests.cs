using Serenity.CodeGenerator;
using static Serenity.Tests.CustomerEntityInputs;

namespace Serenity.Tests.CodeGenerator;

public partial class EntiyModelGeneratorTests
{
    [Fact]
    public void Throws_ArgumentNull_If_Inputs_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new EntityModelGenerator().GenerateModel(inputs: null));
    }

    private string AttrName<TAttr>()
    {
        var fullName = typeof(TAttr).FullName;
        if (fullName.EndsWith("Attribute"))
            return fullName[0..^9];

        return fullName;
    }

    [Fact]
    public void Customer_Defaults()
    {
        var generator = new EntityModelGenerator();
        var inputs = new CustomerEntityInputs();
        var model = generator.GenerateModel(inputs);
        Assert.Equal(Customer, model.ClassName);
        Assert.Equal(TestConnection, model.ConnectionKey);
        Assert.Equal(TestModule, model.Module);
        Assert.Equal(TestPermission, model.Permission);
        Assert.Equal(TestNamespace, model.RootNamespace);
        Assert.Equal(Customer + "Row", model.RowClassName);
        Assert.Equal(TestSchema, model.Schema);
        Assert.Equal(Customer, model.Tablename);
        Assert.Equal(Customer, model.Title);
        Assert.Equal(CustomerId, model.IdField);
        Assert.Equal(typeof(Row<>).FullName.Split('`')[0] + $"<{Customer}Row.RowFields>", model.RowBaseClass);
        Assert.Equal(CustomerName, model.NameField);
        Assert.Equal("", model.FieldPrefix);
        Assert.True(model.AspNetCore);
        Assert.True(model.NET5Plus);
        Assert.False(model.DeclareJoinConstants);

        Assert.Collection(model.Fields,
            customerId =>
            {
                Assert.Equal(CustomerId, customerId.PropertyName);
                Assert.Equal("Int32", customerId.FieldType);
                Assert.Equal("int", customerId.DataType);
                Assert.Equal("number", customerId.TSType);
                Assert.Equal(CustomerId, customerId.Name);
                Assert.Equal("Customer Id", customerId.Title);
                Assert.True(customerId.IsValueType);
                Assert.True(customerId.OmitInForm);
                Assert.Null(customerId.Size);
                Assert.Equal(0, customerId.Scale);

                Assert.Collection(customerId.FlagList, 
                    identity =>
                    {
                        Assert.Equal(AttrName<IdentityAttribute>(), identity.TypeName);
                        Assert.Empty(identity.Arguments);
                    });

                Assert.Collection(customerId.AttributeList,
                    displayName =>
                    {
                        Assert.Equal(AttrName<DisplayNameAttribute>(), displayName.TypeName);
                        Assert.Collection(displayName.Arguments, x => Assert.Equal("Customer Id", x));
                    },
                    identity =>
                    {
                        Assert.Equal(AttrName<IdentityAttribute>(), identity.TypeName);
                        Assert.Empty(identity.Arguments);
                    },
                    idProperty =>
                    {
                        Assert.Equal(AttrName<IdPropertyAttribute>(), idProperty.TypeName);
                        Assert.Empty(idProperty.Arguments);
                    });

                Assert.Collection(customerId.ColAttributeList,
                    editLink =>
                    {
                        Assert.Equal(AttrName<EditLinkAttribute>(), editLink.TypeName);
                        Assert.Empty(editLink.Arguments);
                    },
                    displayName =>
                    {
                        Assert.Equal(AttrName<DisplayNameAttribute>(), displayName.TypeName);
                        Assert.Collection(displayName.Arguments, x => Assert.Equal("Db.Shared.RecordId", x));
                    },
                    alignRight =>
                    {
                        Assert.Equal(AttrName<AlignRightAttribute>(), alignRight.TypeName);
                        Assert.Empty(alignRight.Arguments);
                    });
            },
            customerName =>
            {
                Assert.Equal(CustomerName, customerName.PropertyName);
                Assert.Equal("String", customerName.FieldType);
                Assert.Equal("string", customerName.DataType);
                Assert.Equal("string", customerName.TSType);
                Assert.Equal(CustomerName, customerName.Name);
                Assert.Equal("Customer Name", customerName.Title);
                Assert.False(customerName.IsValueType);
                Assert.False(customerName.OmitInForm);
                Assert.Equal(50, customerName.Size);

                Assert.Collection(customerName.FlagList,
                    notNull =>
                    {
                        Assert.Equal(AttrName<NotNullAttribute>(), notNull.TypeName);
                        Assert.Empty(notNull.Arguments);
                    });

                Assert.Collection(customerName.AttributeList,
                    displayName =>
                    {
                        Assert.Equal(AttrName<DisplayNameAttribute>(), displayName.TypeName);
                        Assert.Collection(displayName.Arguments, x => Assert.Equal("Customer Name", x));
                    },
                    size =>
                    {
                        Assert.Equal(AttrName<SizeAttribute>(), size.TypeName);
                        Assert.Collection(size.Arguments, x => Assert.Equal(50, x));
                    },
                    notNull =>
                    {
                        Assert.Equal(AttrName<NotNullAttribute>(), notNull.TypeName);
                        Assert.Empty(notNull.Arguments);
                    },
                    quickSearch =>
                    {
                        Assert.Equal(AttrName<QuickSearchAttribute>(), quickSearch.TypeName);
                        Assert.Empty(quickSearch.Arguments);
                    },
                    nameProperty =>
                    {
                        Assert.Equal(AttrName<NamePropertyAttribute>(), nameProperty.TypeName);
                        Assert.Empty(nameProperty.Arguments);
                    });

                Assert.Collection(customerName.ColAttributeList,
                    editLink =>
                    {
                        Assert.Equal(AttrName<EditLinkAttribute>(), editLink.TypeName);
                        Assert.Empty(editLink.Arguments);
                    });
            },
            cityId =>
            {
                Assert.Equal(CityId, cityId.PropertyName);
            });
    }

    [Fact]
    public void Customer_DeclareJoinConstants_False_Configuration()
    {
        var generator = new EntityModelGenerator();
        var inputs = new CustomerEntityInputs();
        inputs.Config.DeclareJoinConstants = false;
        var model = generator.GenerateModel(inputs);
        Assert.False(model.DeclareJoinConstants);
        
        var joinAttr = model.Fields.FirstOrDefault(x => x.PropertyName == CityId)?
            .AttributeList?.FirstOrDefault(x => x.TypeName == LeftJoinAttrName);
        Assert.NotNull(joinAttr);
        Assert.Collection(joinAttr.Arguments, x => Assert.Equal($"{jCity}", x));

        var cityJoin = model.Joins.FirstOrDefault(x => x.Name == City);
        Assert.NotNull(cityJoin);
        
        var cityNameExpr = cityJoin.Fields.FirstOrDefault(x => x.PropertyName == CityName)?
            .AttributeList?.FirstOrDefault(x => x.TypeName == ExpressionAttrName);
        Assert.NotNull(cityNameExpr);
        Assert.Collection(cityNameExpr.Arguments, x => Assert.Equal($"{jCity}.[{CityName}]", x));

        var countryIdExpr = cityJoin.Fields.FirstOrDefault(x => x.PropertyName == CityCountryId)?
            .AttributeList?.FirstOrDefault(x => x.TypeName == ExpressionAttrName);
        Assert.NotNull(countryIdExpr);
        Assert.Collection(countryIdExpr.Arguments, x => Assert.Equal($"{jCity}.[{CountryId}]", x));
    }

    [Fact]
    public void Customer_DeclareJoinConstants_True_Configuration()
    {
        var generator = new EntityModelGenerator();
        var inputs = new CustomerEntityInputs();
        inputs.Config.DeclareJoinConstants = true;
        var model = generator.GenerateModel(inputs);
        Assert.True(model.DeclareJoinConstants);

        var joinAttr = model.Fields.FirstOrDefault(x => x.PropertyName == CityId)?
            .AttributeList?.FirstOrDefault(x => x.TypeName == LeftJoinAttrName);
        Assert.NotNull(joinAttr);
        Assert.Collection(joinAttr.Arguments, x => Assert.Equal(jCity, Assert.IsType<RawCode>(x).Code));

        var cityJoin = model.Joins.FirstOrDefault(x => x.Name == City);
        Assert.NotNull(cityJoin);

        var cityNameExpr = cityJoin.Fields.FirstOrDefault(x => x.PropertyName == CityName)?
            .AttributeList?.FirstOrDefault(x => x.TypeName == ExpressionAttrName);
        Assert.NotNull(cityNameExpr);
        Assert.Collection(cityNameExpr.Arguments, 
            x => Assert.Equal("$\"{" + jCity + "}.[" + CityName + "]\"", Assert.IsType<RawCode>(x).Code));

        var countryIdExpr = cityJoin.Fields.FirstOrDefault(x => x.PropertyName == CityCountryId)?
            .AttributeList?.FirstOrDefault(x => x.TypeName == ExpressionAttrName);
        Assert.NotNull(countryIdExpr);
        Assert.Collection(countryIdExpr.Arguments,
            x => Assert.Equal("$\"{" + jCity + "}.[" + CountryId + "]\"", Assert.IsType<RawCode>(x).Code));
    }

    [Fact]
    public void Customer_ForeignFieldSelection_None_No_Include_No_Remove()
    {
        var generator = new EntityModelGenerator();

        var inputs = new CustomerEntityInputs();
        inputs.Config.ForeignFieldSelection = GeneratorConfig.FieldSelection.None;
        inputs.Config.IncludeForeignFields = null;
        inputs.Config.RemoveForeignFields = null;

        var model = generator.GenerateModel(inputs);

        Assert.Collection(model.Joins, x =>
        {
            Assert.Empty(x.Fields);
        });
    }

    [Fact]
    public void Customer_ForeignFieldSelection_None_No_Include_Manual_Exclude()
    {
        var generator = new EntityModelGenerator();

        var inputs = new CustomerEntityInputs();
        inputs.Config.ForeignFieldSelection = GeneratorConfig.FieldSelection.None;
        inputs.Config.IncludeForeignFields = null;
        inputs.Config.RemoveForeignFields = new() { CountryId };

        var model = generator.GenerateModel(inputs);

        Assert.Collection(model.Joins, x =>
        {
            Assert.Empty(x.Fields);
        });
    }

    [Fact]
    public void Customer_ForeignFieldSelection_None_Manual_Include()
    {
        var generator = new EntityModelGenerator();

        var inputs = new CustomerEntityInputs();
        inputs.Config.ForeignFieldSelection = GeneratorConfig.FieldSelection.None;
        inputs.Config.IncludeForeignFields = new() { CountryId };
        inputs.Config.RemoveForeignFields = null;

        var model = generator.GenerateModel(inputs);

        Assert.Collection(model.Joins, join =>
        {
            Assert.Collection(join.Fields, fld =>
            {
                Assert.Equal(CityCountryId, fld.PropertyName);
            });
        });
    }

}
