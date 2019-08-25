import { Component } from 'react';
import * as React from 'react';
import { Avatar } from 'antd';

interface SupportServerState extends React.ComponentState {
    data: object;
}

export class SupportServerInfo extends Component<object, SupportServerState> {
    state = { data: {} };
    async componentWillMount() {
        var r = await (await fetch("api/status/support")).json();

        this.setState({ data: r });
    }
    render() {
        if (!this.state.data['name']) return <div>No support server attached.</div>

        var supportServerTableStyle: React.CSSProperties = {
            WebkitColumnGap: 300
        }
    return (
        <div>
            <h1>Support server</h1>
            <table style={supportServerTableStyle} >
                <tr>
                    <td><Avatar size={64} src={this.state.data['guildIconUrl']} /></td>
                    <td><b>Name:</b> {this.state.data['name']}<br />
                        <b>Owner:</b> {this.state.data['owner']}
                        </td>
                </tr>
            </table>
      </div>
    );
  }
}
