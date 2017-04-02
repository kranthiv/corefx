// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace Microsoft.CSharp.RuntimeBinder.Semantics
{
    internal abstract class ExprVisitorBase
    {
        public Expr Visit(Expr pExpr)
        {
            if (pExpr == null)
            {
                return null;
            }

            Expr pResult;
            if (IsCachedExpr(pExpr, out pResult))
            {
                return pResult;
            }

            if (pExpr is ExprStatement statement)
            {
                return CacheExprMapping(pExpr, DispatchStatementList(statement));
            }

            return CacheExprMapping(pExpr, Dispatch(pExpr));
        }

        /////////////////////////////////////////////////////////////////////////////////

        private ExprStatement DispatchStatementList(ExprStatement expr)
        {
            Debug.Assert(expr != null);

            ExprStatement first = expr;
            ExprStatement pexpr = first;

            while (pexpr != null)
            {
                // If the processor replaces the statement -- potentially with
                // null, another statement, or a list of statements -- then we
                // make sure that the statement list is hooked together correctly.

                ExprStatement next = pexpr.OptionalNextStatement;
                ExprStatement old = pexpr;

                // Unhook the next one.
                pexpr.OptionalNextStatement = null;

                ExprStatement result = Dispatch(pexpr) as ExprStatement;

                if (pexpr == first)
                {
                    first = result;
                }
                else
                {
                    pexpr.OptionalNextStatement = result;
                }

                // A transformation may return back a list of statements (or
                // if the statements have been determined to be unnecessary,
                // perhaps it has simply returned null.)
                //
                // Skip visiting the new list, then hook the tail of the old list
                // up to the end of the new list.

                while (pexpr.OptionalNextStatement != null)
                {
                    pexpr = pexpr.OptionalNextStatement;
                }

                // Re-hook the next pointer.
                pexpr.OptionalNextStatement = next;
            }
            return first;
        }

        /////////////////////////////////////////////////////////////////////////////////

        private bool IsCachedExpr(Expr pExpr, out Expr pTransformedExpr)
        {
            pTransformedExpr = null;
            return false;
        }

        /////////////////////////////////////////////////////////////////////////////////

        private Expr CacheExprMapping(Expr pExpr, Expr pTransformedExpr)
        {
            return pTransformedExpr;
        }

        protected virtual Expr Dispatch(Expr pExpr)
        {
            switch (pExpr.Kind)
            {
                case ExpressionKind.EK_BLOCK:
                    return VisitBLOCK(pExpr as ExprBlock);
                case ExpressionKind.EK_RETURN:
                    return VisitRETURN(pExpr as ExprReturn);
                case ExpressionKind.EK_BINOP:
                    return VisitBINOP(pExpr as ExprBinOp);
                case ExpressionKind.EK_UNARYOP:
                    return VisitUNARYOP(pExpr as ExprUnaryOp);
                case ExpressionKind.EK_ASSIGNMENT:
                    return VisitASSIGNMENT(pExpr as ExprAssignment);
                case ExpressionKind.EK_LIST:
                    return VisitLIST(pExpr as ExprList);
                case ExpressionKind.EK_QUESTIONMARK:
                    return VisitQUESTIONMARK(pExpr as ExprQuestionMark);
                case ExpressionKind.EK_ARRAYINDEX:
                    return VisitARRAYINDEX(pExpr as ExprArrayIndex);
                case ExpressionKind.EK_ARRAYLENGTH:
                    return VisitARRAYLENGTH(pExpr as ExprArrayLength);
                case ExpressionKind.EK_CALL:
                    return VisitCALL(pExpr as ExprCall);
                case ExpressionKind.EK_EVENT:
                    return VisitEVENT(pExpr as ExprEvent);
                case ExpressionKind.EK_FIELD:
                    return VisitFIELD(pExpr as ExprField);
                case ExpressionKind.EK_LOCAL:
                    return VisitLOCAL(pExpr as ExprLocal);
                case ExpressionKind.EK_THISPOINTER:
                    return VisitTHISPOINTER(pExpr as ExprThisPointer);
                case ExpressionKind.EK_CONSTANT:
                    return VisitCONSTANT(pExpr as ExprConstant);
                case ExpressionKind.EK_TYPEARGUMENTS:
                    return VisitTYPEARGUMENTS(pExpr as ExprTypeArguments);
                case ExpressionKind.EK_CLASS:
                    return VisitCLASS(pExpr as ExprClass);
                case ExpressionKind.EK_FUNCPTR:
                    return VisitFUNCPTR(pExpr as ExprFuncPtr);
                case ExpressionKind.EK_PROP:
                    return VisitPROP(pExpr as ExprProperty);
                case ExpressionKind.EK_MULTI:
                    return VisitMULTI(pExpr as ExprMulti);
                case ExpressionKind.EK_MULTIGET:
                    return VisitMULTIGET(pExpr as ExprMultiGet);
                case ExpressionKind.EK_WRAP:
                    return VisitWRAP(pExpr as ExprWrap);
                case ExpressionKind.EK_CONCAT:
                    return VisitCONCAT(pExpr as ExprConcat);
                case ExpressionKind.EK_ARRINIT:
                    return VisitARRINIT(pExpr as ExprArrayInit);
                case ExpressionKind.EK_CAST:
                    return VisitCAST(pExpr as ExprCast);
                case ExpressionKind.EK_USERDEFINEDCONVERSION:
                    return VisitUSERDEFINEDCONVERSION(pExpr as ExprUserDefinedConversion);
                case ExpressionKind.EK_TYPEOF:
                    return VisitTYPEOF(pExpr as ExprTypeOf);
                case ExpressionKind.EK_ZEROINIT:
                    return VisitZEROINIT(pExpr as ExprZeroInit);
                case ExpressionKind.EK_USERLOGOP:
                    return VisitUSERLOGOP(pExpr as ExprUserLogicalOp);
                case ExpressionKind.EK_MEMGRP:
                    return VisitMEMGRP(pExpr as ExprMemberGroup);
                case ExpressionKind.EK_BOUNDLAMBDA:
                    return VisitBOUNDLAMBDA(pExpr as ExprBoundLambda);
                case ExpressionKind.EK_UNBOUNDLAMBDA:
                    return VisitUNBOUNDLAMBDA(pExpr as ExprUnboundLambda);
                case ExpressionKind.EK_HOISTEDLOCALEXPR:
                    return VisitHOISTEDLOCALEXPR(pExpr as ExprHoistedLocalExpr);
                case ExpressionKind.EK_FIELDINFO:
                    return VisitFIELDINFO(pExpr as ExprFieldInfo);
                case ExpressionKind.EK_METHODINFO:
                    return VisitMETHODINFO(pExpr as ExprMethodInfo);

                // Binary operators
                case ExpressionKind.EK_EQUALS:
                    return VisitEQUALS(pExpr as ExprBinOp);
                case ExpressionKind.EK_COMPARE:
                    return VisitCOMPARE(pExpr as ExprBinOp);
                case ExpressionKind.EK_NE:
                    return VisitNE(pExpr as ExprBinOp);
                case ExpressionKind.EK_LT:
                    return VisitLT(pExpr as ExprBinOp);
                case ExpressionKind.EK_LE:
                    return VisitLE(pExpr as ExprBinOp);
                case ExpressionKind.EK_GT:
                    return VisitGT(pExpr as ExprBinOp);
                case ExpressionKind.EK_GE:
                    return VisitGE(pExpr as ExprBinOp);
                case ExpressionKind.EK_ADD:
                    return VisitADD(pExpr as ExprBinOp);
                case ExpressionKind.EK_SUB:
                    return VisitSUB(pExpr as ExprBinOp);
                case ExpressionKind.EK_MUL:
                    return VisitMUL(pExpr as ExprBinOp);
                case ExpressionKind.EK_DIV:
                    return VisitDIV(pExpr as ExprBinOp);
                case ExpressionKind.EK_MOD:
                    return VisitMOD(pExpr as ExprBinOp);
                case ExpressionKind.EK_BITAND:
                    return VisitBITAND(pExpr as ExprBinOp);
                case ExpressionKind.EK_BITOR:
                    return VisitBITOR(pExpr as ExprBinOp);
                case ExpressionKind.EK_BITXOR:
                    return VisitBITXOR(pExpr as ExprBinOp);
                case ExpressionKind.EK_LSHIFT:
                    return VisitLSHIFT(pExpr as ExprBinOp);
                case ExpressionKind.EK_RSHIFT:
                    return VisitRSHIFT(pExpr as ExprBinOp);
                case ExpressionKind.EK_LOGAND:
                    return VisitLOGAND(pExpr as ExprBinOp);
                case ExpressionKind.EK_LOGOR:
                    return VisitLOGOR(pExpr as ExprBinOp);
                case ExpressionKind.EK_SEQUENCE:
                    return VisitSEQUENCE(pExpr as ExprBinOp);
                case ExpressionKind.EK_SEQREV:
                    return VisitSEQREV(pExpr as ExprBinOp);
                case ExpressionKind.EK_SAVE:
                    return VisitSAVE(pExpr as ExprBinOp);
                case ExpressionKind.EK_SWAP:
                    return VisitSWAP(pExpr as ExprBinOp);
                case ExpressionKind.EK_INDIR:
                    return VisitINDIR(pExpr as ExprBinOp);
                case ExpressionKind.EK_STRINGEQ:
                    return VisitSTRINGEQ(pExpr as ExprBinOp);
                case ExpressionKind.EK_STRINGNE:
                    return VisitSTRINGNE(pExpr as ExprBinOp);
                case ExpressionKind.EK_DELEGATEEQ:
                    return VisitDELEGATEEQ(pExpr as ExprBinOp);
                case ExpressionKind.EK_DELEGATENE:
                    return VisitDELEGATENE(pExpr as ExprBinOp);
                case ExpressionKind.EK_DELEGATEADD:
                    return VisitDELEGATEADD(pExpr as ExprBinOp);
                case ExpressionKind.EK_DELEGATESUB:
                    return VisitDELEGATESUB(pExpr as ExprBinOp);
                case ExpressionKind.EK_EQ:
                    return VisitEQ(pExpr as ExprBinOp);

                // Unary operators
                case ExpressionKind.EK_TRUE:
                    return VisitTRUE(pExpr as ExprUnaryOp);
                case ExpressionKind.EK_FALSE:
                    return VisitFALSE(pExpr as ExprUnaryOp);
                case ExpressionKind.EK_INC:
                    return VisitINC(pExpr as ExprUnaryOp);
                case ExpressionKind.EK_DEC:
                    return VisitDEC(pExpr as ExprUnaryOp);
                case ExpressionKind.EK_LOGNOT:
                    return VisitLOGNOT(pExpr as ExprUnaryOp);
                case ExpressionKind.EK_NEG:
                    return VisitNEG(pExpr as ExprUnaryOp);
                case ExpressionKind.EK_UPLUS:
                    return VisitUPLUS(pExpr as ExprUnaryOp);
                case ExpressionKind.EK_BITNOT:
                    return VisitBITNOT(pExpr as ExprUnaryOp);
                case ExpressionKind.EK_ADDR:
                    return VisitADDR(pExpr as ExprUnaryOp);
                case ExpressionKind.EK_DECIMALNEG:
                    return VisitDECIMALNEG(pExpr as ExprUnaryOp);
                case ExpressionKind.EK_DECIMALINC:
                    return VisitDECIMALINC(pExpr as ExprUnaryOp);
                case ExpressionKind.EK_DECIMALDEC:
                    return VisitDECIMALDEC(pExpr as ExprUnaryOp);
                default:
                    throw Error.InternalCompilerError();
            }
        }

        private void VisitChildren(Expr pExpr)
        {
            Debug.Assert(pExpr != null);

            Expr exprRet;

            switch (pExpr.Kind)
            {
                case ExpressionKind.EK_LIST:

                    // Lists are a special case.  We treat a list not as a
                    // binary node but rather as a node with n children.
                    ExprList list = (ExprList)pExpr;
                    while (true)
                    {
                        list.OptionalElement = Visit(list.OptionalElement);
                        Expr nextNode = list.OptionalNextListNode;
                        if (nextNode == null)
                        {
                            return;
                        }

                        if (!(nextNode is ExprList next))
                        {
                            list.OptionalNextListNode = Visit(nextNode);
                            return;
                        }

                        list = next;
                    }

                case ExpressionKind.EK_ASSIGNMENT:
                    exprRet = Visit((pExpr as ExprAssignment).LHS);
                    Debug.Assert(exprRet != null);
                    (pExpr as ExprAssignment).LHS = exprRet;
                    exprRet = Visit((pExpr as ExprAssignment).RHS);
                    Debug.Assert(exprRet != null);
                    (pExpr as ExprAssignment).RHS = exprRet;
                    break;

                case ExpressionKind.EK_QUESTIONMARK:
                    exprRet = Visit((pExpr as ExprQuestionMark).TestExpression);
                    Debug.Assert(exprRet != null);
                    (pExpr as ExprQuestionMark).TestExpression = exprRet;
                    exprRet = Visit((pExpr as ExprQuestionMark).Consequence);
                    Debug.Assert(exprRet != null);
                    (pExpr as ExprQuestionMark).Consequence = exprRet as ExprBinOp;
                    break;

                case ExpressionKind.EK_ARRAYINDEX:
                    exprRet = Visit((pExpr as ExprArrayIndex).Array);
                    Debug.Assert(exprRet != null);
                    (pExpr as ExprArrayIndex).Array = exprRet;
                    exprRet = Visit((pExpr as ExprArrayIndex).Index);
                    Debug.Assert(exprRet != null);
                    (pExpr as ExprArrayIndex).Index = exprRet;
                    break;

                case ExpressionKind.EK_ARRAYLENGTH:
                    exprRet = Visit((pExpr as ExprArrayLength).Array);
                    Debug.Assert(exprRet != null);
                    (pExpr as ExprArrayLength).Array = exprRet;
                    break;

                case ExpressionKind.EK_UNARYOP:
                case ExpressionKind.EK_TRUE:
                case ExpressionKind.EK_FALSE:
                case ExpressionKind.EK_INC:
                case ExpressionKind.EK_DEC:
                case ExpressionKind.EK_LOGNOT:
                case ExpressionKind.EK_NEG:
                case ExpressionKind.EK_UPLUS:
                case ExpressionKind.EK_BITNOT:
                case ExpressionKind.EK_ADDR:
                case ExpressionKind.EK_DECIMALNEG:
                case ExpressionKind.EK_DECIMALINC:
                case ExpressionKind.EK_DECIMALDEC:
                    exprRet = Visit((pExpr as ExprUnaryOp).Child);
                    Debug.Assert(exprRet != null);
                    (pExpr as ExprUnaryOp).Child = exprRet;
                    break;

                case ExpressionKind.EK_USERLOGOP:
                    exprRet = Visit((pExpr as ExprUserLogicalOp).TrueFalseCall);
                    Debug.Assert(exprRet != null);
                    (pExpr as ExprUserLogicalOp).TrueFalseCall = exprRet;
                    exprRet = Visit((pExpr as ExprUserLogicalOp).OperatorCall);
                    Debug.Assert(exprRet != null);
                    (pExpr as ExprUserLogicalOp).OperatorCall = exprRet as ExprCall;
                    exprRet = Visit((pExpr as ExprUserLogicalOp).FirstOperandToExamine);
                    Debug.Assert(exprRet != null);
                    (pExpr as ExprUserLogicalOp).FirstOperandToExamine = exprRet;
                    break;

                case ExpressionKind.EK_TYPEOF:
                    exprRet = Visit((pExpr as ExprTypeOf).SourceType);
                    (pExpr as ExprTypeOf).SourceType = exprRet as ExprClass;
                    break;

                case ExpressionKind.EK_CAST:
                    exprRet = Visit((pExpr as ExprCast).Argument);
                    Debug.Assert(exprRet != null);
                    (pExpr as ExprCast).Argument = exprRet;
                    exprRet = Visit((pExpr as ExprCast).DestinationType);
                    (pExpr as ExprCast).DestinationType = exprRet as ExprClass;
                    break;

                case ExpressionKind.EK_USERDEFINEDCONVERSION:
                    exprRet = Visit((pExpr as ExprUserDefinedConversion).UserDefinedCall);
                    Debug.Assert(exprRet != null);
                    (pExpr as ExprUserDefinedConversion).UserDefinedCall = exprRet;
                    break;

                case ExpressionKind.EK_ZEROINIT:
                    exprRet = Visit((pExpr as ExprZeroInit).OptionalArgument);
                    (pExpr as ExprZeroInit).OptionalArgument = exprRet;

                    // Used for when we zeroinit 0 parameter constructors for structs/enums.
                    exprRet = Visit((pExpr as ExprZeroInit).OptionalConstructorCall);
                    (pExpr as ExprZeroInit).OptionalConstructorCall = exprRet;
                    break;

                case ExpressionKind.EK_BLOCK:
                    exprRet = Visit((pExpr as ExprBlock).OptionalStatements);
                    (pExpr as ExprBlock).OptionalStatements = exprRet as ExprStatement;
                    break;

                case ExpressionKind.EK_MEMGRP:

                    // The object expression. NULL for a static invocation.
                    exprRet = Visit((pExpr as ExprMemberGroup).OptionalObject);
                    (pExpr as ExprMemberGroup).OptionalObject = exprRet;
                    break;

                case ExpressionKind.EK_CALL:
                    exprRet = Visit((pExpr as ExprCall).OptionalArguments);
                    (pExpr as ExprCall).OptionalArguments = exprRet;
                    exprRet = Visit((pExpr as ExprCall).MemberGroup);
                    Debug.Assert(exprRet != null);
                    (pExpr as ExprCall).MemberGroup = exprRet as ExprMemberGroup;
                    break;

                case ExpressionKind.EK_PROP:
                    exprRet = Visit((pExpr as ExprProperty).OptionalArguments);
                    (pExpr as ExprProperty).OptionalArguments = exprRet;
                    exprRet = Visit((pExpr as ExprProperty).MemberGroup);
                    Debug.Assert(exprRet != null);
                    (pExpr as ExprProperty).MemberGroup = exprRet as ExprMemberGroup;
                    break;

                case ExpressionKind.EK_FIELD:
                    exprRet = Visit((pExpr as ExprField).OptionalObject);
                    (pExpr as ExprField).OptionalObject = exprRet;
                    break;

                case ExpressionKind.EK_EVENT:
                    exprRet = Visit((pExpr as ExprEvent).OptionalObject);
                    (pExpr as ExprEvent).OptionalObject = exprRet;
                    break;

                case ExpressionKind.EK_RETURN:
                    exprRet = Visit((pExpr as ExprReturn).OptionalObject);
                    (pExpr as ExprReturn).OptionalObject = exprRet;
                    break;

                case ExpressionKind.EK_CONSTANT:

                    // Used for when we zeroinit 0 parameter constructors for structs/enums.
                    exprRet = Visit((pExpr as ExprConstant).OptionalConstructorCall);
                    (pExpr as ExprConstant).OptionalConstructorCall = exprRet;
                    break;

                /*************************************************************************************************
                  TYPEEXPRs defined:

                  The following exprs are used to represent the results of type binding, and are defined as follows:

                  TYPEARGUMENTS - This wraps the type arguments for a class. It contains the TypeArray* which is
                    associated with the AggregateType for the instantiation of the class. 

                  TYPEORNAMESPACE - This is the base class for this set of Exprs. When binding a type, the result
                    must be a type or a namespace. This Expr encapsulates that fact. The lhs member is the Expr 
                    tree that was bound to resolve the type or namespace.

                  TYPEORNAMESPACEERROR - This is the error class for the type or namespace exprs when we don't know
                    what to bind it to.

                  The following three exprs all have a TYPEORNAMESPACE child, which is their fundamental type:
                    POINTERTYPE - This wraps the sym for the pointer type.
                    NULLABLETYPE - This wraps the sym for the nullable type.

                  CLASS - This represents an instantiation of a class.

                  NSPACE - This represents a namespace, which is the intermediate step when attempting to bind
                    a qualified name.

                  ALIAS - This represents an alias

                *************************************************************************************************/

                case ExpressionKind.EK_TYPEARGUMENTS:
                    exprRet = Visit((pExpr as ExprTypeArguments).OptionalElements);
                    (pExpr as ExprTypeArguments).OptionalElements = exprRet;
                    break;

                case ExpressionKind.EK_MULTI:
                    exprRet = Visit((pExpr as ExprMulti).Left);
                    Debug.Assert(exprRet != null);
                    (pExpr as ExprMulti).Left = exprRet;
                    exprRet = Visit((pExpr as ExprMulti).Operator);
                    Debug.Assert(exprRet != null);
                    (pExpr as ExprMulti).Operator = exprRet;
                    break;

                case ExpressionKind.EK_CONCAT:
                    exprRet = Visit((pExpr as ExprConcat).FirstArgument);
                    Debug.Assert(exprRet != null);
                    (pExpr as ExprConcat).FirstArgument = exprRet;
                    exprRet = Visit((pExpr as ExprConcat).SecondArgument);
                    Debug.Assert(exprRet != null);
                    (pExpr as ExprConcat).SecondArgument = exprRet;
                    break;

                case ExpressionKind.EK_ARRINIT:
                    exprRet = Visit((pExpr as ExprArrayInit).OptionalArguments);
                    (pExpr as ExprArrayInit).OptionalArguments = exprRet;
                    exprRet = Visit((pExpr as ExprArrayInit).OptionalArgumentDimensions);
                    (pExpr as ExprArrayInit).OptionalArgumentDimensions = exprRet;
                    break;

                case ExpressionKind.EK_BOUNDLAMBDA:
                    exprRet = Visit((pExpr as ExprBoundLambda).OptionalBody);
                    (pExpr as ExprBoundLambda).OptionalBody = exprRet as ExprBlock;
                    break;

                case ExpressionKind.EK_LOCAL:
                case ExpressionKind.EK_THISPOINTER:
                case ExpressionKind.EK_CLASS:
                case ExpressionKind.EK_FUNCPTR:
                case ExpressionKind.EK_MULTIGET:
                case ExpressionKind.EK_WRAP:
                case ExpressionKind.EK_NOOP:
                case ExpressionKind.EK_UNBOUNDLAMBDA:
                case ExpressionKind.EK_HOISTEDLOCALEXPR:
                case ExpressionKind.EK_FIELDINFO:
                case ExpressionKind.EK_METHODINFO:
                    break;

                default:
                    pExpr.AssertIsBin();
                    exprRet = Visit((pExpr as ExprBinOp).OptionalLeftChild);
                    (pExpr as ExprBinOp).OptionalLeftChild = exprRet;
                    exprRet = Visit((pExpr as ExprBinOp).OptionalRightChild);
                    (pExpr as ExprBinOp).OptionalRightChild = exprRet;
                    break;
            }
        }

        protected virtual Expr VisitEXPR(Expr pExpr)
        {
            VisitChildren(pExpr);
            return pExpr;
        }
        protected virtual Expr VisitBLOCK(ExprBlock pExpr)
        {
            return VisitSTMT(pExpr);
        }
        protected virtual Expr VisitTHISPOINTER(ExprThisPointer pExpr)
        {
            return VisitLOCAL(pExpr);
        }
        protected virtual Expr VisitRETURN(ExprReturn pExpr)
        {
            return VisitSTMT(pExpr);
        }
        protected virtual Expr VisitCLASS(ExprClass pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitSTMT(ExprStatement pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitBINOP(ExprBinOp pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitLIST(ExprList pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitASSIGNMENT(ExprAssignment pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitQUESTIONMARK(ExprQuestionMark pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitARRAYINDEX(ExprArrayIndex pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitARRAYLENGTH(ExprArrayLength pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitUNARYOP(ExprUnaryOp pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitUSERLOGOP(ExprUserLogicalOp pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitTYPEOF(ExprTypeOf pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitCAST(ExprCast pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitUSERDEFINEDCONVERSION(ExprUserDefinedConversion pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitZEROINIT(ExprZeroInit pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitMEMGRP(ExprMemberGroup pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitCALL(ExprCall pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitPROP(ExprProperty pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitFIELD(ExprField pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitEVENT(ExprEvent pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitLOCAL(ExprLocal pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitCONSTANT(ExprConstant pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitTYPEARGUMENTS(ExprTypeArguments pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitFUNCPTR(ExprFuncPtr pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitMULTIGET(ExprMultiGet pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitMULTI(ExprMulti pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitWRAP(ExprWrap pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitCONCAT(ExprConcat pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitARRINIT(ExprArrayInit pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitBOUNDLAMBDA(ExprBoundLambda pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitUNBOUNDLAMBDA(ExprUnboundLambda pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitHOISTEDLOCALEXPR(ExprHoistedLocalExpr pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitFIELDINFO(ExprFieldInfo pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitMETHODINFO(ExprMethodInfo pExpr)
        {
            return VisitEXPR(pExpr);
        }
        protected virtual Expr VisitEQUALS(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitCOMPARE(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitEQ(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitNE(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitLE(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitGE(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitADD(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitSUB(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitDIV(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitBITAND(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitBITOR(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitLSHIFT(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitLOGAND(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitSEQUENCE(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitSAVE(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitINDIR(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitSTRINGEQ(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitDELEGATEEQ(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitDELEGATEADD(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitRANGE(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitLT(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitMUL(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitBITXOR(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitRSHIFT(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitLOGOR(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitSEQREV(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitSTRINGNE(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitDELEGATENE(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitGT(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitMOD(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitSWAP(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitDELEGATESUB(ExprBinOp pExpr)
        {
            return VisitBINOP(pExpr);
        }
        protected virtual Expr VisitTRUE(ExprUnaryOp pExpr)
        {
            return VisitUNARYOP(pExpr);
        }
        protected virtual Expr VisitINC(ExprUnaryOp pExpr)
        {
            return VisitUNARYOP(pExpr);
        }
        protected virtual Expr VisitLOGNOT(ExprUnaryOp pExpr)
        {
            return VisitUNARYOP(pExpr);
        }
        protected virtual Expr VisitNEG(ExprUnaryOp pExpr)
        {
            return VisitUNARYOP(pExpr);
        }
        protected virtual Expr VisitBITNOT(ExprUnaryOp pExpr)
        {
            return VisitUNARYOP(pExpr);
        }
        protected virtual Expr VisitADDR(ExprUnaryOp pExpr)
        {
            return VisitUNARYOP(pExpr);
        }
        protected virtual Expr VisitDECIMALNEG(ExprUnaryOp pExpr)
        {
            return VisitUNARYOP(pExpr);
        }
        protected virtual Expr VisitDECIMALDEC(ExprUnaryOp pExpr)
        {
            return VisitUNARYOP(pExpr);
        }
        protected virtual Expr VisitFALSE(ExprUnaryOp pExpr)
        {
            return VisitUNARYOP(pExpr);
        }
        protected virtual Expr VisitDEC(ExprUnaryOp pExpr)
        {
            return VisitUNARYOP(pExpr);
        }
        protected virtual Expr VisitUPLUS(ExprUnaryOp pExpr)
        {
            return VisitUNARYOP(pExpr);
        }
        protected virtual Expr VisitDECIMALINC(ExprUnaryOp pExpr)
        {
            return VisitUNARYOP(pExpr);
        }
    }
}
