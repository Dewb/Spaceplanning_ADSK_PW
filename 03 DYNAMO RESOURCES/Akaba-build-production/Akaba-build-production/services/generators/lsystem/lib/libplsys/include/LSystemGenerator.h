//
//  LSystemGenerator.h
//  burningbush
//
//  Created by Dewb on 1/5/15.
//
//

#ifndef burningbush_LSystemGenerator_h
#define burningbush_LSystemGenerator_h

#include "LSystem.h"
#include <stack>
#include <iostream>

#ifdef USE_CXX_TR1
#include <tr1/functional>
using tr1::function;
#else
#include <functional>
#endif

typedef vector<float> FloatParams;

template<typename StateType>
class SymbolData {
public:
    typedef function<void(StateType&, FloatParams&)> SymbolAction;
    
    SymbolData(RuleToken t) : token(t) { _startsGroup = false; _endsGroup = false;}
    SymbolData& startsGroup() { _startsGroup = true; return *this; }
    SymbolData& endsGroup() { _endsGroup = true; return *this; }
    SymbolData& action(const SymbolAction& action) { _actions.push_back(action); return *this; }
    
    bool shouldStartGroup() const { return _startsGroup; }
    bool shouldEndGroup() const { return _endsGroup; }
    
    const RuleToken& getToken() const { return token; }
    
    void execute(StateType& state, FloatParams& params) const { for (auto& action : _actions) { action(state, params); } }
protected:
    RuleToken token;
    vector<SymbolAction> _actions;
    bool _startsGroup;
    bool _endsGroup;
private:
    SymbolData(); // unimplemented
};

template<typename StateType>
class Generator {
public:
    typedef function<void(StateType&, FloatParams)> SymbolAction;
    typedef SymbolData<StateType> Symbol;
    
    void add(const Symbol& token) { _symbols.insert(std::pair<RuleToken, Symbol>(token.getToken(), token)); }
    
    const Symbol* getSymbol(const RuleToken& token) {
        for (auto iter = _symbols.begin(); iter != _symbols.end(); iter++) {
            if (token.symbol == iter->first.symbol) {
                return &(iter->second);
            }
        }
        return NULL;
    }
    
    virtual void begin(StateType& state) {}
    virtual void end(StateType& state) {};
    
    void generate(LSystem& system, StateType& state, unsigned iterations, int steps = -1) {
        RuleString ruleStr = system.generate(iterations);
        if (steps < 0 || steps > ruleStr.size()) {
            steps = ruleStr.size();
        }
        stack<StateType> stateStack;
        stateStack.push(state);
        begin(stateStack.top());
        auto iter = ruleStr.begin();
        unsigned i = 0;
        while(i < steps && iter != ruleStr.end()) {
            auto sym = getSymbol(*iter);
            if (sym) {
                if (sym->shouldStartGroup()) {
                    StateType newState = stateStack.top();
                    stateStack.push(newState);
                }
                
                FloatParams floatParams;
                for (auto& stringParam : iter->parameters) {
                    stringstream ss(stringParam);
                    float x;
                    ss >> x;
                    if (ss.fail()) {
                        cout << "ERROR: parameter " << stringParam << " is non-numeric!\n";
                    } else {
                        floatParams.push_back(x);
                    }
                }
                
                sym->execute(stateStack.top(), floatParams);
                
                if (sym->shouldEndGroup() && stateStack.size() > 1) {
                    stateStack.pop();
                }
            }
            i++;
            ++iter;
        }
        end(stateStack.top());
        state = stateStack.top();
    }
    
protected:
    map<RuleToken, Symbol> _symbols;
};

#endif
