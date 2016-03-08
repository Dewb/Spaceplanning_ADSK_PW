//
//  LSystemRulesEngine.cpp
//  burningbush
//
//  Created by Michael Dewberry on 1/29/15.
//
//

#include "LSystemRulesEngine.h"
#include "exprtk/exprtk.hpp"
#include <iostream>

#ifdef USE_CXX_TR1
#include <tr1/tuple>
using tr1::tuple;
using tr1::make_tuple;
#else
#include <tuple>
#endif

template <typename T>
class ExpressionWrapper {
public:
    
    string expressionString;
    
    typedef exprtk::symbol_table<T>     symbol_table_t;
    typedef exprtk::expression<T>       expression_t;
    typedef exprtk::parser<T>           parser_t;
    typedef typename parser_t::dependent_entity_collector::symbol_t symbol_t;
    
    parser_t parser;
    symbol_table_t symbol_table;
    expression_t expression;
    std::deque<symbol_t> symbol_list;
    
    bool isParsed;
    bool isBound;
    
    ExpressionWrapper(string expr) {
        expressionString = expr;
        expression.register_symbol_table(symbol_table);
        parser.dec().collect_variables() = true;
        parser.enable_unknown_symbol_resolver();
        isParsed = false;
        isBound = false;
        
        if (!parser.compile(expressionString, expression)) {
            cout << "Failed to compile expression: " << expressionString << ", error: " << parser.error() << "\n";
            return;
        }
        
        isParsed = true;
        parser.dec().symbols(symbol_list);
    }
    
    bool bind(const vector<string> formalParams, const vector<string> arguments) {
        
        for (std::size_t i = 0; i < symbol_list.size(); ++i) {
            symbol_t& symbol = symbol_list[i];
            
            switch (symbol.second) {
                case parser_t::e_st_variable:
                    {
                        bool found = false;
                        for (int i = 0; i < formalParams.size(); i++) {
                            if (formalParams[i] == symbol.first) {
                                T x;
                                stringstream ss(arguments[i]);
                                ss >> x;
                                if (ss.fail()) {
                                    cout << "ERROR: Argument " << arguments[i] << " is non-numeric!\n";
                                    return false;
                                }
                                
                                //cout << "Assigning " << symbol.first << " to " << x << "\n";
                                symbol_table.variable_ref(symbol.first) = x;
                                found = true;
                            }
                        }
                        if (!found) {
                            cout << "ERROR: Unknown symbol " << symbol.first << " in expression " << expressionString << "\n";
                            return false;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        
        isBound = true;
        return true;
    }
    
    T getValue() {
        if (!isParsed) {
           cout << "ERROR: Asking for value of unparsed expression " << expressionString << "\n";
        } else if (!isBound) {
            cout << "ERROR: Asking for value of unbound expression " << expressionString << "\n";
        }
        return expression.value();
    }
};

typedef ExpressionWrapper<float> Expression;

class ExpressionCache {
public:
    ExpressionCache(const RuleSet& rules);
    ~ExpressionCache();
    Expression* getCondition(const RuleToken& token, int ruleIndex);
    Expression* getSuccessor(const RuleToken& token, int ruleIndex, int tokenIndex, int argIndex);
    
    map<tuple<RuleToken, int>, Expression*> conditionExpressions;
    map<tuple<RuleToken, int, int, int>, Expression*> successorExpressions;
};

Expression* ExpressionCache::getCondition(const RuleToken& token, int ruleIndex) {
    auto t = make_tuple(token, ruleIndex);
    auto iter = conditionExpressions.find(t);
    if (iter != conditionExpressions.end()) {
        return iter->second;
    } else {
        return NULL;
    }
}

Expression* ExpressionCache::getSuccessor(const RuleToken& token, int ruleIndex, int tokenIndex, int argIndex) {
    auto t = make_tuple(token, ruleIndex, tokenIndex, argIndex);
    auto iter = successorExpressions.find(t);
    if (iter != successorExpressions.end()) {
        return iter->second;
    } else {
        return NULL;
    }
}

ExpressionCache::ExpressionCache(const RuleSet& rules) {
    for (auto& r : rules) {
        const RuleToken& token = r.first;
        for (int ruleIndex = 0; ruleIndex < r.second.size(); ruleIndex++) {
            auto& rule = r.second[ruleIndex];
            if (!rule.parametricCondition.empty()) {
                auto e = new Expression(rule.parametricCondition);
                conditionExpressions.insert(make_pair(make_tuple(token, ruleIndex), e));
            }
            int tokenIndex = 0;
            for (auto iter = rule.successor.begin(); iter != rule.successor.end(); iter++) {
                auto args = iter->parameters;
                for (int argIndex = 0; argIndex < args.size(); argIndex++) {
                    auto e = new Expression(args[argIndex]);
                    successorExpressions.insert(make_pair(make_tuple(token, ruleIndex, tokenIndex, argIndex), e));
                }
                tokenIndex++;
            }
        }
    }
}

ExpressionCache::~ExpressionCache() {
    for (auto& item : conditionExpressions) {
        delete item.second;
    }
    for (auto& item : successorExpressions) {
        delete item.second;
    }
}

float evaluateExpression(const RuleToken& predecessor, const RuleToken& tokenMatch, Expression* e) {
    if (e->isParsed) {
        if (e->bind(predecessor.parameters, tokenMatch.parameters)) {
            return e->getValue();
        }
    }
    return 0;
}

bool conditionMatches(const RuleToken& predecessor, const RuleToken& tokenMatch, Expression* e) {
    return evaluateExpression(predecessor, tokenMatch, e) != 0.0;
}

LSystemRulesEngine::LSystemRulesEngine(LSystem* ls) {
    system = ls;
    expressionCache = new ExpressionCache(ls->rules);
}

LSystemRulesEngine::~LSystemRulesEngine() {
    delete expressionCache;
}

RuleString LSystemRulesEngine::evaluateSuccessor(int ruleIndex, const RuleToken& predecessor, const RuleToken& tokenMatch, const RuleString& successor) {
    if (!predecessor.isParametric()) {
        return successor;
    }
    RuleString result = successor;
    int tokenIndex = 0;
    for (auto& token : result) {
        for (int i = 0; i < token.parameters.size(); i++) {
            Expression* e = expressionCache->getSuccessor(predecessor, ruleIndex, tokenIndex, i);
            float value = evaluateExpression(predecessor, tokenMatch, e);
            token.parameters[i] = to_string(value);
        }
        tokenIndex++;
    }
    return result;
}


template <typename Iter>
bool contextMatches(const RuleString& ignoreContext, const Iter& contextBegin, const Iter& contextEnd,
                             const Iter& stringBegin, const Iter& stringEnd,
                             bool reversed, bool followBranches, int* pTrunkLength = NULL) {
    auto currentPos = stringBegin;
    auto contextPos = contextBegin;
    char startBranch = '[';
    char endBranch = ']';
    if (reversed) {
        swap(startBranch, endBranch);
    }
    int trunkLength = 0;
    
    while (currentPos != stringEnd && contextPos != contextEnd) {
        if(find(ignoreContext.begin(), ignoreContext.end(), *currentPos) != ignoreContext.end()) {
            // skip tokens in ignore string
            ++currentPos;
        } else if (*currentPos == endBranch) {
            if (followBranches) {
                // branch ended without a context match
                return false;
            } else {
                // continue on parent
                ++currentPos;
            }
        } else if (*currentPos == startBranch) {
            // starting branch definition, find the end
            int closeBracketsRequired = 1;
            auto branchEnd = currentPos;
            ++branchEnd;
            while (closeBracketsRequired > 0) {
                if (*branchEnd == startBranch) {
                    closeBracketsRequired++;
                } else if (*branchEnd == endBranch) {
                    closeBracketsRequired--;
                }
                ++branchEnd;
            }
            if (followBranches) {
                // check for a match on both branches
                auto insideStart = ++currentPos;
                auto insideEnd = branchEnd;
                insideEnd--;
                int moreTrunkLength = 0;
                // Prefer staying on this trunk/branch
                if (contextMatches(ignoreContext, contextPos, contextEnd, branchEnd, stringEnd, reversed, followBranches, &moreTrunkLength)) {
                    if (pTrunkLength) {
                        *pTrunkLength = trunkLength + moreTrunkLength;
                    }
                    return true;
                } else if (contextMatches(ignoreContext, contextPos, contextEnd, insideStart, insideEnd, reversed, followBranches)) {
                    if (pTrunkLength) {
                        *pTrunkLength = trunkLength;
                    }
                    return true;
                }
            } else {
                // skip ahead to end
                currentPos = branchEnd;
            }
        } else if (*contextPos != *currentPos) {
            // context did not match
            return false;
        } else {
            // context does match so far
            currentPos++;
            contextPos++;
            trunkLength++;
        }
    }
    if (contextPos == contextEnd) {
        if (pTrunkLength) {
            *pTrunkLength = trunkLength;
        }
        return true;
    } else {
        return false;
    }
}

void LSystemRulesEngine::getMatchingRules(const RuleString& current, const RuleString::iterator& currentPos, IndexedProductionRuleGroup& matched) {
    if (system->rules.find(*currentPos) == system->rules.end()) {
        return;
    }
    
    auto nextPos = currentPos;
    nextPos++;
    
    // If a context-free and a contextual rule match, prefer the contextual;
    // if multiple contextual rules match, prefer the longer one;
    // if multiple contextual rules of the same length match, prefer the one on the axial branch (trunk)
    IndexedProductionRuleGroup contextFreeMatches;
    IndexedProductionRuleGroup contextualMatches;
    int contextMatchLength = 0;
    int trunkMatchLength = 0;
    
    auto& ruleSet = system->rules[*currentPos];
    for (int ruleIndex = 0; ruleIndex < ruleSet.size(); ruleIndex++) {
        auto& rule = ruleSet[ruleIndex];
        if (rule.leftContext.empty() && rule.rightContext.empty()) {
            if (!rule.isParametric() || rule.parametricCondition.empty()) {
                contextFreeMatches.push_back(make_pair(ruleIndex, rule));
            } else {
                Expression* e = expressionCache->getCondition(*currentPos, ruleIndex);
                if (conditionMatches(rule.predecessor, *currentPos, e)) {
                    contextFreeMatches.push_back(make_pair(ruleIndex, rule));
                }
            }
        } else {
            if (!rule.leftContext.empty() &&
                !contextMatches<RuleString::const_reverse_iterator>(
                    system->ignoreContext,
                    rule.leftContext.rbegin(), rule.leftContext.rend(),
                    reverse_iterator<RuleString::iterator>(currentPos), current.rend(),
                    true, false))
            {
                continue;
            }
            int rightTrunkLength = 0;
            if (!rule.rightContext.empty() &&
                !contextMatches<RuleString::const_iterator>(
                    system->ignoreContext,
                    rule.rightContext.begin(), rule.rightContext.end(),
                    nextPos, current.end(),
                    false, true, &rightTrunkLength))
            {
                continue;
            }
            
            int contextLength = rule.leftContext.size() + rule.rightContext.size();
            int trunkLength = rule.leftContext.size() + rightTrunkLength;
            
            if (contextLength > contextMatchLength) {
                contextualMatches.clear();
                contextualMatches.push_back(make_pair(ruleIndex, rule));
                contextMatchLength = contextLength;
                trunkMatchLength = trunkLength;
            } else if (contextLength == contextMatchLength) {
                if (trunkLength > trunkMatchLength) {
                    contextualMatches.clear();
                    contextualMatches.push_back(make_pair(ruleIndex, rule));
                    trunkMatchLength = trunkLength;
                } else if (trunkLength == trunkMatchLength) {
                    contextualMatches.push_back(make_pair(ruleIndex, rule));
                }
            }
        }
    }
    
    if (contextualMatches.size()) {
        matched = contextualMatches;
    } else {
        matched = contextFreeMatches;
    }
}

