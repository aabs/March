module FromSong
{
export Common;

    language Common
    {
        // Parameterized List rule        
        syntax List(element) 
            = n:element => [n] 
            | n:element l:List(element) => [n, valuesof(l)];
        
        syntax List(element, separator) 
            = n:element => [n] 
            | n:element separator l:List(element, separator) => [n, valuesof(l)];
            
        // Whitespace
        syntax LF = "\u000A";
        syntax CR = "\u000D";
        syntax Space = "\u0020";
        syntax Whitespace = LF | CR | Space;
    }
}

// the main hierarchy is - Artefacts (types of programs), Applications (specific programs) and Applicaiton instances (installed runnable programs)
        //      Feature declaration needs to contain either a simple feature list (constrained) or to
        //      contain a feature with overriding/completing property declarations
module Languages {
    import FromSong;
    import Language;
    export AppSpec;
    
/*
    IDEAS
    =====
    1. Need to introduce pragmas to allow the specification of things like reserved attributes to describe
       known information (like third-party-product, creator, self-installing etc)
*/
    language AppSpec 
    {

        // token for various symbols and characters etc
        @{Classification["Delimiter"]} token TSemiColon = ";";
        @{Classification["Delimiter"]} token TColon = ":";
        @{Classification["Delimiter"]} token TComma = ",";
        token TLetter = Base.Letter;
        @{Classification["Operator"]} token GETS = "=";
        @{Classification["Operator"]} token EQ = "==";
        @{Classification["Operator"]} token NEQ = "!=";
        @{Classification["Operator"]} token LT = "<";
        @{Classification["Operator"]} token GT = ">";
        @{Classification["Operator"]} token GE = ">=";
        @{Classification["Operator"]} token LE = "<=";
        @{Classification["Operator"]} token IN = "~=";
        @{Classification["Operator"]} token NOTIN = "~!=";
        @{Classification["Delimiter"]} token LParen = "(";
        @{Classification["Delimiter"]} token RParen = ")";
        @{Classification["Delimiter"]} token LBrack = "[";
        @{Classification["Delimiter"]} token RBrack = "]";        
        @{Classification["Delimiter"]} token LABrack = "<";
        @{Classification["Delimiter"]} token RABrack = ">";        
        @{Classification["Delimiter"]} token LBrace = "{";
        @{Classification["Delimiter"]} token RBrace = "}";
        
        @{Classification["Operator"]} token TPlus = "+";
        @{Classification["Operator"]} token TMinus = "-";
        @{Classification["Operator"]} token TMul = "*";
        @{Classification["Operator"]} token TDiv = "/";
        @{Classification["Operator"]} token TMod = "%";

        @{Classification["Operator"]} token TAnd = "&&";
        @{Classification["Operator"]} token TOr = "||";
        
        @{Classification["Identifier"]}
        identifier token ValidIdent = Base.Letter (Base.Letter | Base.Digit | '_')*;
           
        token BinaryOperator = TAnd | TOr | EQ | LT | GT | GE | LE | NEQ | IN | NOTIN | TPlus | TMinus | TMul | TDiv | TMod;        
        
        // keywords
        @{Classification["Keyword"]} token TImport = "use";
        @{Classification["Keyword"]} token TDefine = "define";
        @{Classification["Keyword"]} token TArtefact = "artefact";
        @{Classification["Keyword"]} token TPublic = "public";
        @{Classification["Keyword"]} token TPrivate = "private";
        @{Classification["Keyword"]} token TWhere = "where";
        @{Classification["Keyword"]} token TDefault = "default";
        @{Classification["Keyword"]} token TEnum = "enum";
        @{Classification["Keyword"]} token TRequires = "requires";
        @{Classification["Keyword"]} token TApplication = "application";
        @{Classification["Keyword"]} token TAssembly = "assembly";
        @{Classification["Keyword"]} token TDatabase = "database";
        @{Classification["Keyword"]} token TNetwork = "network";
        @{Classification["Keyword"]} token TServer = "server";
        @{Classification["Keyword"]} token TFeatures = "features";
        @{Classification["Keyword"]} token TProlog = "architecture";
        @{Classification["Keyword"]} token TAuthor = "author";
        @{Classification["Keyword"]} token TDate = "date";
        @{Classification["Keyword"]} token TVersion = "version";
        @{Classification["Keyword"]} token TDesc = "notes";
        
        @{Classification["Keyword"]} token TTInt = "int";
        @{Classification["Keyword"]} token TTReal = "real";
        @{Classification["Keyword"]} token TTDate = "date";
        @{Classification["Keyword"]} token TTDuration = "duration";
        @{Classification["Keyword"]} token TTGuid = "guid";
        @{Classification["Keyword"]} token TTString = "string";
        @{Classification["Keyword"]} token TTBlob = "blob";
        @{Classification["Keyword"]} token TTRef = "reference";
        
        token TTypeKeyword = TTInt | TTReal | TTDate | TTDuration | TTGuid | TTString | TTBlob | TTRef;
        
        // whitespace and comments
        interleave Skippable = Base.Whitespace | Language.Grammar.Comment;
        

        // the main entry point to the grammar
        syntax Main =   p:Prolog 
                        i:Common.List(Import)?
                        d:Common.List(Definition)? 
                        a:Common.List(ArtefactItem)
                => Architecture{
                        Prolog{p}, 
                        Imports{[valuesof(i)]}, 
                        Definitions{[valuesof(d)]}, 
                        Artefacts{[valuesof(a)]}
                   };

        syntax ArtefactItem = 
                  ar:ArtefactDefinition    =>ar 
                | en:EnumDefinition        =>en 
                | ap:ApplicationDefinition =>ap 
                | db:DatabaseDeclaration   =>db 
                | nw:NetworkDeclaration    =>nw;
                
        // Artefacts
        syntax ArtefactDefinition = 
            TArtefact i:Identifier x:(TColon il:IdentifierList=>il )? LBrace d:PropertyDefinition* y:ArtefactDefinition* RBrace
            => Artefact {
                 Name{i}, 
                 Bases{[valuesof(x)]}, 
                 PropertyDefinitions{[valuesof(d)]},
                 Body{[valuesof(y)]}
               };

        syntax SimpleNameValuePair(prefix, dataType) =
            prefix TColon a:dataType TSemiColon => a
        ;
        
        syntax Prolog = TProlog LBrace 
                    a:SimpleNameValuePair(TAuthor, Language.Grammar.TextLiteral)
                    d:SimpleNameValuePair(TDate, Language.Grammar.Date)
                    v:SimpleNameValuePair(TVersion, Language.Grammar.TextLiteral)?
                    desc:SimpleNameValuePair(TDesc, Language.Grammar.TextLiteral)?
                    RBrace
            => Prolog{
                Author{a}, 
                DateCreated{d}, 
                Version{v}, 
                Description{desc}
            };
        
        syntax ArtefactList = 
            im:ArtefactItem => Artefacts[im]
            | il:ArtefactList im:ArtefactItem => ArtefactList[valuesof(il), im];
        
        syntax DefinitionList = 
            im:Definition => DefinitionList[im]
            | il:DefinitionList im:Definition => DefinitionList[valuesof(il), im];

        syntax ImportList = 
            im:Import => Imports[im]
            | il:ImportList im:Import => ImportList[valuesof(il), im];
            
        syntax Import = TImport imported:Grammar.TextLiteral TSemiColon => imported;

        syntax Definition = TDefine i:Identifier l:Grammar.TextLiteral TSemiColon => Definition{Name{i}, Value{l}};

        // general purpose helpers in declarations and definitions
        syntax VisibilityModifier = TPublic | TPrivate;
        
        syntax IdentifierList = il:Common.List(DottedIdentifier, ",") =>IdentifierList[valuesof(il)];
        
        syntax TypeConstraint = 
            TWhere i:Identifier TColon d:Expression 
				=> Constraint{
						Subject{i}, 
						ConstrainedTo{d}
					};
        
        syntax DefaultConstraint = 
            TDefault TColon v:Grammar.TextLiteral 
				=> v;
        
        syntax DependencyDeclaration = TRequires LBrace i:Common.List(DottedIdentifier, ',') RBrace
            => Dependencies[valuesof(i)];
        
        // Property definitions and declarations
        syntax PropertyDefinition = 
              i:Identifier TColon d1:TypeSpecification TSemiColon 
                  => PropertyDefinition {
						Name{i}, 
						Type{d1}
					}
            | i:Identifier TColon d1:TypeSpecification c1:TypeConstraint TSemiColon 
                  => PropertyDefinition{
						Name{i}, 
						Type{d1}, 
						TypeConstraint{c1}
					}
            | i:Identifier TColon d1:TypeSpecification d:DefaultConstraint TSemiColon 
                  => PropertyDefinition{
						Name{i}, 
						Type{d1}, 
						Default{d}
					}
            | i:Identifier TColon d1:TypeSpecification c1:TypeConstraint d:DefaultConstraint TSemiColon 
                  => PropertyDefinition{
						Name{i}, 
						Type{d1}, 
						TypeConstraint{c1}, 
						Default{d}
					};
        
        syntax TypeSpecification = t:TTypeKeyword => Type{t};
        
        syntax PropertyDeclaration = i:Identifier GETS x:Expression TSemiColon 
                => PropertyDeclaration{
                    Name{i}, 
                    Value{x}
                }; 
        
        // Applications
        
        syntax AppDef(Prefix) =
                x:Prefix i:Identifier bases:(TColon b:IdentifierList => b)? 
                LBrace propDef:PropertyDefinition* propDec:PropertyDeclaration* appDef:ApplicationDefinition* dependencies:DependencyDeclaration? RBrace
             => Application{
                    ApplicationType{x},
					Name{i}, 
					PropertyDefinitions{[valuesof(propDef)]},
					PropertyDeclarations{[valuesof(propDec)]},
					Bases{[valuesof(bases)]},
					Dependencies{[valuesof(dependencies)]},
					Applications{[valuesof(appDef)]}
				};
        syntax ApplicationDefinition =
                    a:AppDef(TApplication)=>a
                    |a:AppDef(TAssembly)=>a
                    |a:AppDef(TDatabase)=>a;

             
        // Databases
        syntax DatabaseDeclaration = 
          TDatabase i:Identifier b:(TColon b:IdentifierList=>b)? "{" p:PropertyDeclaration* "}"
            => Database{Name{i}, Bases[b], Properties[valuesof(p)]}
        | TDatabase i:Identifier b:(TColon b:IdentifierList=>b)? "{" p:PropertyDeclaration* r:DependencyDeclaration "}"
            => Database{Name{i}, Bases[b], Properties[valuesof(p)], r};
        
        // Networks and Servers
        syntax NetworkDeclaration = TNetwork i:Identifier LBrace p:PropertyDeclaration* s:ServerDeclaration+ RBrace
            => Network{
					DomainName{i}, 
					Servers{[valuesof(s)]}
			   };
            
        syntax ServerDeclaration = TServer i:Identifier bases:(TColon b:IdentifierList => b)? 
				LBrace p:PropertyDeclaration* f:FeatureList? RBrace
            =>Server{
				DomainName{i}, 
				PropertyDeclarations{[valuesof(p)]}, 
				Features{[valuesof(f)]}
				};

        syntax FeatureList = 
            TFeatures LBrace x:Common.List(ApplicationInstanceDeclaration)  RBrace
            => FeatureList[valuesof(x)] ;

        // Application Instances
        syntax ApplicationInstanceDeclaration = 
				i:ConstrainedDottedIdentifier 
				d:(LBrace pd:PropertyDeclaration* RBrace=>[valuesof(pd)])? TSemiColon
				=> ApplicationInstanceDeclaration{ 
						Name{i}, 
						PropertyDeclarations{[valuesof(d)]}
					};

        // Enums
        syntax EnumDefinition = TEnum i:Identifier LBrace il:IdentifierList RBrace 
            => Enumeration{Name{i}, Values{[valuesof(il)]}};
            
        // expression handling
        syntax BinaryOperation = 
          i:Identifier op:BinaryOperator l:Grammar.TextLiteral => Op{Operator{op}, Identifier{i}, Literal{l}}
        | i:Identifier op:BinaryOperator z:Grammar.Integer => Op{Operator{op}, Identifier{i}, Number{z}};
        
        syntax Expression =
              v:Value => v
            | v:XPathReference => v
            | LParen e:Expression RParen => e
            | l:Expression op:BinaryOperator r:Expression 
                => BinaryExpression{
                    Op{op}, 
                    Left{l}, 
                    Right{r}
                };
        syntax Value = 
              x:DottedIdentifier => ReferenceExpression{Value{x}}
            | x:Language.Grammar.DateTime => DateTime{Value{x}}
            | x:Language.Grammar.DateTimeOffset => Duration{Value{x}}
            | x:Language.Grammar.Decimal => Real{Value{x}}
            | x:Language.Grammar.Guid => Guid{Value{x}}
            | x:Language.Grammar.Integer => Int{Value{x}}
            | x:Language.Grammar.Scientific => Real{Value{x}}
            | x:Language.Grammar.TextLiteral => MyString{Value{x}}
            | x:Language.Grammar.Binary => Blob{Value{x}};
            
        syntax XPathReference = LABrack src:Identifier RABrack "." LABrack x:Language.Grammar.TextLiteral RABrack 
			=> XPath{
					Source{src}, 
					Path{x}
			};

// ##################[IDENTIFIERS]########################
        syntax Identifier = x:ValidIdent
                => Identifier{
                        Value{[x]}
                   };
        
        syntax DottedIdentifier = 
            i:Common.List(ValidIdent, ".") 
                => Identifier{
                        Value{[valuesof(i)]}
                   };
        
        syntax ConstrainedIdentifierSegment = 
            i:Identifier
				=> ConstrainedIdentifierSegment{
						IdentifierConstrained{i}
					}
            | i:Identifier LBrack c:Expression RBrack
				=> ConstrainedIdentifierSegment{
						IdentifierConstrained{i}, 
						Constraint{c}
					};
        
        syntax ConstrainedDottedIdentifier = il:Common.List(ConstrainedIdentifierSegment, ".")
            => ConstrainedDottedIdentifier{
                Value{[valuesof(il)]}
            };

        syntax FeatureConstraint = LBrack i:Identifier EQ exp:Expression RBrack 
				=> Constraint{
					Subject{i},
					ConstrainedTo{exp}
				};
                
    }   
}

