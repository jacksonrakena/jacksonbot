import { Component } from 'react';
import * as React from 'react';
import Button from 'antd/es/button';
import { BotStatus } from '../types/status';

interface HomeState extends React.ComponentState {
    status: BotStatus;
}

export class Home extends Component<object, HomeState> {
    state = { status: {} as BotStatus};
    async componentWillMount() {
        var req = await (await fetch("api/status")).json();
        this.setState({ status: req });
    }

    render() {
        return (
            <div>
                <Button type="primary">Click</Button>
                <Button type="danger">Terminate</Button>
            </div>
        );
    }
}
