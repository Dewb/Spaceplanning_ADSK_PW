//
//  ProductionRule.cpp
//  burningbush
//
//  Created by Michael Dewberry on 1/30/15.
//
//

#include "LSystem.h"


ProductionRule::ProductionRule(const RuleToken& pred, const RuleString& succ)
: predecessor(pred), successor(succ), probability(1.0) {
}

ProductionRule& ProductionRule::setContext(const string& left, const string& right) {
    leftContext = left;
    rightContext = right;
    return *this;
}

ProductionRule& ProductionRule::setLeftContext(const string& context) {
    leftContext = context;
    return *this;
}

ProductionRule& ProductionRule::setRightContext(const string& context) {
    rightContext = context;
    return *this;
}

ProductionRule& ProductionRule::setCondition(const string& condition) {
    parametricCondition = condition;
    return *this;
}

ProductionRule& ProductionRule::setProbability(float p) {
    probability = p;
    return *this;
}

bool ProductionRuleGroup::isStochastic() const {
    for (auto& item : *this) {
        if (item.isStochastic()) {
            return true;
        }
    }
    return false;
}

std::ostream& operator<<(std::ostream& os, const ProductionRule& rule) {
    if (!rule.leftContext.empty()) {
        os << rule.leftContext;
        os << " < ";
    }
    os << rule.predecessor;
    if (!rule.rightContext.empty()) {
        os << " > ";
        os << rule.rightContext;
    }
    if (!rule.parametricCondition.empty()) {
        os << " : ";
        os << rule.parametricCondition;
    }
    os << " -> ";
    os << rule.successor;
    return os;
}

string to_string(const ProductionRule& pr) {
    ostringstream ss;
    ss << pr;
    return ss.str();
}


