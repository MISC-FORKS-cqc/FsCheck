namespace FsCheck.Test

module TypeClass =

    open System
    open Xunit
    open FsCheck
    open FsCheck.TypeClass
    open Swensen.Unquote

    type ITypeClassUnderTest<'a> =
        abstract GetSomething : int

    [<Fact>]
    let ``should be empty on initialization``() =
        let typeClassDef = TypeClass<ITypeClassUnderTest<_>>.New()
        typedefof<ITypeClassUnderTest<_>> =! typeClassDef.Class
        test <@ typeClassDef.Instances.IsEmpty @>
        false =! typeClassDef.HasCatchAll

    [<Fact>]
    let ``should throw when intialized with non-generic type``() =
        raises<Exception> <@ TypeClass<string>.New() @>

    type PrimitiveInstance() =
        static member Int() =
            { new ITypeClassUnderTest<int> with
                override __.GetSomething = 1 }
        //to check that methods with arguments are ignored
        static member Int2(_:string) =
            { new ITypeClassUnderTest<int> with
                override __.GetSomething = 3 }

    [<Fact>]
    let ``should discover primitive types``() = 
        let typeClass = 
            TypeClass<ITypeClassUnderTest<_>>
                .New()
                .Discover(true, typeof<PrimitiveInstance>)
        1 =! typeClass.Instances.Count
        test <@ typeClass.Instances.Contains (Primitive typeof<int>) @>
        false =! typeClass.HasCatchAll

    type ArrayInstance() =
        static member Array2() =
            { new ITypeClassUnderTest<'a[,]> with
                override __.GetSomething = 2 }

    [<Fact>]
    let ``should discover array types``() = 
        let typeClass = 
            TypeClass<ITypeClassUnderTest<_>>
                .New()
                .Discover(true, typeof<ArrayInstance>)
        1 =! typeClass.Instances.Count
        test <@ typeClass.Instances.Contains (Array typeof<_[,]>) @>

    type CatchAllInstance() =
        static member CatchAll() =
            { new ITypeClassUnderTest<'a> with
                override __.GetSomething = 3 }

    [<Fact>]
    let ``should discover catchall``() = 
        let typeClass = 
            TypeClass<ITypeClassUnderTest<_>>
                .New()
                .Discover(true, typeof<CatchAllInstance>)
        test <@ typeClass.HasCatchAll @>
        1 =! typeClass.Instances.Count

    [<Fact>]
    let ``should instantiate primitive type``() =
        let instance = 
            TypeClass<ITypeClassUnderTest<_>>
                .New()
                .Discover(true, typeof<PrimitiveInstance>)
                .InstanceFor<int,ITypeClassUnderTest<int>>()

        1 =! instance.GetSomething

    [<Fact>]
    let ``should instantiate array type``() =
        let instance = 
            TypeClass<ITypeClassUnderTest<_>>
                .New()
                .Discover(true, typeof<ArrayInstance>)
                .DiscoverAndMerge(true, typeof<PrimitiveInstance>) //so the int is defined too
                .InstanceFor<int[,],ITypeClassUnderTest<int[,]>>()
        2 =! instance.GetSomething

    [<Fact>]
    let ``should instantiate unknown type using catchall``() =
        let instance = 
            TypeClass<ITypeClassUnderTest<_>>
                .New()
                .Discover(true, typeof<CatchAllInstance>)
                .InstanceFor<string,ITypeClassUnderTest<string>>() //string not defined explicitly
        3 =! instance.GetSomething






    
        